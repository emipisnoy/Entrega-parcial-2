using System;
using System.Collections.Generic;
using FlexSpace.DAL;

namespace FlexSpace.BLL
{
    // ==========================================
    // EXCEPCIONES Y DTOs
    // ==========================================
    public class ClienteSancionadoException : Exception
    {
        public ClienteSancionadoException(string nombre, int sanciones)
            : base("El cliente " + nombre + " tiene " + sanciones + " sanciones activas y no puede realizar reservas.")
        {
        }
    }

    public class ClienteInfo
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public int Sanciones { get; set; }
    }

    public class ReservaInfo
    {
        public int Id { get; set; }
        public string Cliente { get; set; }
        public string Puesto { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public decimal Costo { get; set; }
    }

    public class Presupuesto
    {
        public string Cliente { get; set; }
        public string Puesto { get; set; }
        public decimal Horas { get; set; }
        public decimal Subtotal { get; set; }
        public decimal RecargoFinDeSemana { get; set; }
        public decimal DescuentoVolumen { get; set; }
        public decimal DescuentoVip { get; set; }
        public decimal PenalizacionSanciones { get; set; }
        public decimal Total { get; set; }
    }

    // ==========================================
    // LÓGICA DE NEGOCIO (BLL)
    // ==========================================
    public class ClienteBLL
    {
        private ClienteDAL clienteDAL = new ClienteDAL();

        public ClienteBLL()
        {
            Conexion.CrearTablas();
        }

        public List<ClienteInfo> ListarSancionados()
        {
            List<ClienteInfo> lista = new List<ClienteInfo>();
            foreach (Cliente c in clienteDAL.ObtenerSancionados())
            {
                ClienteInfo info = new ClienteInfo();
                info.Id = c.Id;
                info.Nombre = c.Nombre;
                info.Tipo = c.TipoCliente.ToString();
                info.Sanciones = c.SancionesActivas;
                lista.Add(info);
            }
            return lista;
        }
    }

    public class ReservaBLL
    {
        private ClienteDAL clienteDAL = new ClienteDAL();
        private PuestoDAL puestoDAL = new PuestoDAL();
        private ReservaDAL reservaDAL = new ReservaDAL();

        public ReservaBLL()
        {
            Conexion.CrearTablas();
        }

        public Presupuesto Presupuestar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
        {
            if (fin <= inicio)
            {
                throw new Exception("La fecha de fin debe ser posterior a la de inicio.");
            }

            Cliente cliente = clienteDAL.ObtenerPorId(clienteId);
            if (cliente == null)
            {
                throw new Exception("El cliente no existe.");
            }

            // TODO: Falta validar que si el cliente tiene 3 o mas sanciones activas
            // lance la excepcion ClienteSancionadoException para bloquear la reserva.

            Puesto puesto = puestoDAL.ObtenerPorId(puestoId);
            if (puesto == null)
            {
                throw new Exception("El puesto no existe.");
            }

            if (reservaDAL.HaySolapamiento(puestoId, inicio, fin))
            {
                throw new Exception("El puesto ya se encuentra reservado en ese rango horario.");
            }

            return CalcularPresupuesto(cliente, puesto, inicio, fin);
        }

        public int Reservar(int clienteId, int puestoId, DateTime inicio, DateTime fin)
        {
            Presupuesto p = Presupuestar(clienteId, puestoId, inicio, fin);

            Reserva reserva = new Reserva();
            reserva.ClienteId = clienteId;
            reserva.PuestoId = puestoId;
            reserva.FechaInicio = inicio;
            reserva.FechaFin = fin;
            reserva.Estado = EstadoReserva.Confirmada;
            reserva.CostoTotal = p.Total;

            return reservaDAL.Insertar(reserva);
        }

        public bool Cancelar(int reservaId)
        {
            Reserva reserva = reservaDAL.ObtenerPorId(reservaId);
            if (reserva == null)
            {
                throw new Exception("La reserva no existe.");
            }

            reservaDAL.Cancelar(reservaId);

            // TODO: Implementar la logica para verificar si se cancelo con menos de 2 hs de anticipacion
            // y en ese caso sumar la sancion correspondiente al cliente en la base de datos.
            return false;
        }

        private Presupuesto CalcularPresupuesto(Cliente cliente, Puesto puesto, DateTime inicio, DateTime fin)
        {
            Presupuesto p = new Presupuesto();
            p.Cliente = cliente.Nombre;
            p.Puesto = puesto.Codigo;
            p.Horas = Math.Round((decimal)(fin - inicio).TotalHours, 2);

            // 1. Subtotal base
            p.Subtotal = p.Horas * puesto.TarifaBasePorHora;
            decimal total = p.Subtotal;

            // 2. Recargo fin de semana (15%)
            if (IncluyeFinDeSemana(inicio, fin))
            {
                p.RecargoFinDeSemana = p.Subtotal * 0.15m;
                total += p.RecargoFinDeSemana;
            }

            // TODO: Falta agregar los descuentos acumulativos:
            // - Descuento por volumen (10% si horas >= 5)
            // - Descuento VIP (5% si cliente.TipoCliente == TipoCliente.VIP)
            // - Recargo por penalizaciones (+20% sobre la tarifa base si cliente.SancionesActivas > 0)

            p.Total = total;
            return p;
        }

        private bool IncluyeFinDeSemana(DateTime inicio, DateTime fin)
        {
            for (DateTime dia = inicio.Date; dia <= fin.Date; dia = dia.AddDays(1))
            {
                if (dia.DayOfWeek == DayOfWeek.Saturday || dia.DayOfWeek == DayOfWeek.Sunday)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
