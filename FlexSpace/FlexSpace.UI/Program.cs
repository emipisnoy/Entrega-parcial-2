using System;
using System.Collections.Generic;
using FlexSpace.BLL;

namespace FlexSpace.UI
{
    class Program
    {
        private static ReservaBLL reservaBLL = new ReservaBLL();
        private static ClienteBLL clienteBLL = new ClienteBLL();

        static void Main(string[] args)
        {
            string opcion = "";

            do
            {
                Console.Clear();
                Console.WriteLine("===== FlexSpace - Gestor de Coworking =====");
                Console.WriteLine("1. Registrar nueva reserva");
                Console.WriteLine("2. Cancelar reserva");
                Console.WriteLine("3. Consultar reservas activas");
                Console.WriteLine("4. Listar clientes sancionados");
                Console.WriteLine("0. Salir");
                Console.Write("\nSeleccione una opción: ");

                opcion = Console.ReadLine();
                Console.WriteLine();

                switch (opcion)
                {
                    case "1":
                        RegistrarReserva();
                        break;
                    case "2":
                        CancelarReserva();
                        break;
                    case "3":
                        Console.WriteLine("Módulo de consulta en desarrollo.");
                        break;
                    case "4":
                        ListarClientesSancionados();
                        break;
                    case "0":
                        Console.WriteLine("¡Gracias por utilizar FlexSpace!");
                        break;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }

                if (opcion != "0")
                {
                    Console.WriteLine("\nPresione cualquier tecla para continuar...");
                    Console.ReadKey();
                }

            } while (opcion != "0");
        }

        static void RegistrarReserva()
        {
            try
            {
                Console.WriteLine("--- NUEVA RESERVA ---");

                Console.Write("ID del cliente: ");
                if (!int.TryParse(Console.ReadLine(), out int clienteId))
                {
                    Console.WriteLine("El ID del cliente debe ser un número entero.");
                    return;
                }

                Console.Write("ID del puesto: ");
                if (!int.TryParse(Console.ReadLine(), out int puestoId))
                {
                    Console.WriteLine("El ID del puesto debe ser un número entero.");
                    return;
                }

                Console.Write("Fecha y hora de inicio (yyyy-MM-dd HH:mm): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime inicio))
                {
                    Console.WriteLine("Formato de fecha de inicio inválido.");
                    return;
                }

                Console.Write("Fecha y hora de fin (yyyy-MM-dd HH:mm): ");
                if (!DateTime.TryParse(Console.ReadLine(), out DateTime fin))
                {
                    Console.WriteLine("Formato de fecha de fin inválido.");
                    return;
                }

                // 1. Mostrar Presupuesto preliminar
                Presupuesto p = reservaBLL.Presupuestar(clienteId, puestoId, inicio, fin);

                Console.WriteLine("\n--- DESGLOSE DEL PRESUPUESTO ---");
                Console.WriteLine("Cliente: " + p.Cliente);
                Console.WriteLine("Puesto: " + p.Puesto);
                Console.WriteLine("Horas reservadas: " + p.Horas);
                Console.WriteLine("Subtotal: $" + p.Subtotal);
                if (p.RecargoFinDeSemana > 0)
                {
                    Console.WriteLine("Recargo fin de semana: $" + p.RecargoFinDeSemana);
                }
                Console.WriteLine("TOTAL A PAGAR: $" + p.Total);

                // 2. Confirmación
                Console.Write("\n¿Desea confirmar la reserva? (S/N): ");
                string respuesta = Console.ReadLine()?.Trim().ToUpper();

                if (respuesta == "S")
                {
                    int idReserva = reservaBLL.Reservar(clienteId, puestoId, inicio, fin);
                    Console.WriteLine("\n¡Reserva registrada con éxito! ID de reserva: " + idReserva);
                }
                else
                {
                    Console.WriteLine("\nReserva cancelada por el usuario.");
                }
            }
            catch (ClienteSancionadoException ex)
            {
                Console.WriteLine("\n[REGLA DE NEGOCIO - SANCIONADO]: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al registrar la reserva: " + ex.Message);
            }
        }

        static void CancelarReserva()
        {
            try
            {
                Console.WriteLine("--- CANCELAR RESERVA ---");
                Console.Write("ID de la reserva: ");
                if (!int.TryParse(Console.ReadLine(), out int reservaId))
                {
                    Console.WriteLine("El ID de la reserva debe ser un número entero.");
                    return;
                }

                bool aplicoSancion = reservaBLL.Cancelar(reservaId);
                Console.WriteLine("Reserva cancelada correctamente.");

                if (aplicoSancion)
                {
                    Console.WriteLine("Atención: La cancelación se realizó con menos de 2 horas de anticipación y se ha aplicado una sanción al cliente.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al cancelar la reserva: " + ex.Message);
            }
        }

        static void ListarClientesSancionados()
        {
            try
            {
                Console.WriteLine("--- CLIENTES SANCIONADOS ---");
                List<ClienteInfo> sancionados = clienteBLL.ListarSancionados();

                if (sancionados.Count == 0)
                {
                    Console.WriteLine("No hay clientes con sanciones activas.");
                    return;
                }

                foreach (var c in sancionados)
                {
                    Console.WriteLine("ID: " + c.Id + " | Nombre: " + c.Nombre + " | Tipo: " + c.Tipo + " | Sanciones: " + c.Sanciones);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\nError al obtener los clientes sancionados: " + ex.Message);
            }
        }
    }
}