using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Data.Sqlite;

namespace FlexSpace.DAL
{
    // ==========================================
    // ENUMS Y ENTIDADES
    // ==========================================
    public enum TipoCliente
    {
        Estandar,
        VIP
    }

    public enum TipoPuesto
    {
        EscritorioIndividual,
        SalaReuniones,
        CabinaPrivada
    }

    public enum EstadoReserva
    {
        Confirmada,
        Cancelada,
        Finalizada
    }

    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public TipoCliente TipoCliente { get; set; }
        public int SancionesActivas { get; set; }
    }

    public class Puesto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public TipoPuesto TipoPuesto { get; set; }
        public decimal TarifaBasePorHora { get; set; }
    }

    public class Reserva
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public int PuestoId { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoReserva Estado { get; set; }
        public decimal CostoTotal { get; set; }
    }

    // ==========================================
    // CONEXIÓN Y DATOS (DAL)
    // ==========================================
    public class Conexion
    {
        private const string Cadena = "Data Source=flexspace.db";
        private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

        public static SqliteConnection Abrir()
        {
            SqliteConnection con = new SqliteConnection(Cadena);
            con.Open();
            return con;
        }

        public static string FechaATexto(DateTime fecha)
        {
            return fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
        }

        public static DateTime TextoAFecha(string texto)
        {
            return DateTime.ParseExact(texto, FormatoFecha, CultureInfo.InvariantCulture);
        }

        public static void CrearTablas()
        {
            using (SqliteConnection con = Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText =
                    "CREATE TABLE IF NOT EXISTS Clientes (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, Nombre TEXT NOT NULL, Email TEXT NOT NULL, " +
                    "TipoCliente TEXT NOT NULL, SancionesActivas INTEGER NOT NULL DEFAULT 0);" +
                    "CREATE TABLE IF NOT EXISTS Puestos (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, Codigo TEXT NOT NULL UNIQUE, " +
                    "TipoPuesto TEXT NOT NULL, TarifaBasePorHora REAL NOT NULL);" +
                    "CREATE TABLE IF NOT EXISTS Reservas (" +
                    "Id INTEGER PRIMARY KEY AUTOINCREMENT, ClienteId INTEGER NOT NULL, PuestoId INTEGER NOT NULL, " +
                    "FechaInicio TEXT NOT NULL, FechaFin TEXT NOT NULL, Estado TEXT NOT NULL, CostoTotal REAL NOT NULL);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT COUNT(*) FROM Clientes";
                int cantidad = Convert.ToInt32(cmd.ExecuteScalar());
                if (cantidad == 0)
                {
                    cmd.CommandText =
                        "INSERT INTO Clientes (Nombre, Email, TipoCliente, SancionesActivas) VALUES " +
                        "('Ana Perez', 'ana@mail.com', 'Estandar', 0), " +
                        "('Bruno Gomez', 'bruno@mail.com', 'VIP', 0), " +
                        "('Carla Ruiz', 'carla@mail.com', 'Estandar', 1), " +
                        "('Diego Soto', 'diego@mail.com', 'VIP', 3);" +
                        "INSERT INTO Puestos (Codigo, TipoPuesto, TarifaBasePorHora) VALUES " +
                        "('ESC-01', 'EscritorioIndividual', 1000), " +
                        "('SAL-01', 'SalaReuniones', 3500), " +
                        "('CAB-01', 'CabinaPrivada', 2000);";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    public class ClienteDAL
    {
        public Cliente ObtenerPorId(int id)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id, Nombre, Email, TipoCliente, SancionesActivas FROM Clientes WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                SqliteDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    return Leer(dr);
                }
                return null;
            }
        }

        public List<Cliente> ObtenerSancionados()
        {
            List<Cliente> lista = new List<Cliente>();
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id, Nombre, Email, TipoCliente, SancionesActivas FROM Clientes WHERE SancionesActivas > 0";
                SqliteDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(Leer(dr));
                }
            }
            return lista;
        }

        public void SumarSancion(int clienteId)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "UPDATE Clientes SET SancionesActivas = SancionesActivas + 1 WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", clienteId);
                cmd.ExecuteNonQuery();
            }
        }

        private Cliente Leer(SqliteDataReader dr)
        {
            Cliente c = new Cliente();
            c.Id = dr.GetInt32(0);
            c.Nombre = dr.GetString(1);
            c.Email = dr.GetString(2);
            c.TipoCliente = (TipoCliente)Enum.Parse(typeof(TipoCliente), dr.GetString(3));
            c.SancionesActivas = dr.GetInt32(4);
            return c;
        }
    }

    public class PuestoDAL
    {
        public Puesto ObtenerPorId(int id)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id, Codigo, TipoPuesto, TarifaBasePorHora FROM Puestos WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                SqliteDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    return Leer(dr);
                }
                return null;
            }
        }

        public Puesto ObtenerPorCodigo(string codigo)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id, Codigo, TipoPuesto, TarifaBasePorHora FROM Puestos WHERE Codigo = @codigo COLLATE NOCASE";
                cmd.Parameters.AddWithValue("@codigo", codigo);
                SqliteDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    return Leer(dr);
                }
                return null;
            }
        }

        private Puesto Leer(SqliteDataReader dr)
        {
            Puesto p = new Puesto();
            p.Id = dr.GetInt32(0);
            p.Codigo = dr.GetString(1);
            p.TipoPuesto = (TipoPuesto)Enum.Parse(typeof(TipoPuesto), dr.GetString(2));
            p.TarifaBasePorHora = Convert.ToDecimal(dr.GetValue(3));
            return p;
        }
    }

    public class ReservaDAL
    {
        public bool HaySolapamiento(int puestoId, DateTime inicio, DateTime fin)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM Reservas WHERE PuestoId = @puesto AND Estado = 'Confirmada' " +
                                  "AND FechaInicio < @fin AND FechaFin > @inicio";
                cmd.Parameters.AddWithValue("@puesto", puestoId);
                cmd.Parameters.AddWithValue("@inicio", Conexion.FechaATexto(inicio));
                cmd.Parameters.AddWithValue("@fin", Conexion.FechaATexto(fin));
                int cantidad = Convert.ToInt32(cmd.ExecuteScalar());
                return cantidad > 0;
            }
        }

        public int Insertar(Reserva reserva)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "INSERT INTO Reservas (ClienteId, PuestoId, FechaInicio, FechaFin, Estado, CostoTotal) " +
                                  "VALUES (@cliente, @puesto, @inicio, @fin, @estado, @costo)";
                cmd.Parameters.AddWithValue("@cliente", reserva.ClienteId);
                cmd.Parameters.AddWithValue("@puesto", reserva.PuestoId);
                cmd.Parameters.AddWithValue("@inicio", Conexion.FechaATexto(reserva.FechaInicio));
                cmd.Parameters.AddWithValue("@fin", Conexion.FechaATexto(reserva.FechaFin));
                cmd.Parameters.AddWithValue("@estado", reserva.Estado.ToString());
                cmd.Parameters.AddWithValue("@costo", (double)reserva.CostoTotal);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "SELECT last_insert_rowid()";
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public Reserva ObtenerPorId(int id)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id, ClienteId, PuestoId, FechaInicio, FechaFin, Estado, CostoTotal FROM Reservas WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                SqliteDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    return Leer(dr);
                }
                return null;
            }
        }

        public List<Reserva> ObtenerFuturasPorPuesto(int puestoId, DateTime desde)
        {
            List<Reserva> lista = new List<Reserva>();
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "SELECT Id, ClienteId, PuestoId, FechaInicio, FechaFin, Estado, CostoTotal FROM Reservas " +
                                  "WHERE PuestoId = @puesto AND Estado = 'Confirmada' AND FechaInicio > @desde ORDER BY FechaInicio";
                cmd.Parameters.AddWithValue("@puesto", puestoId);
                cmd.Parameters.AddWithValue("@desde", Conexion.FechaATexto(desde));
                SqliteDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    lista.Add(Leer(dr));
                }
            }
            return lista;
        }

        public void Cancelar(int id)
        {
            using (SqliteConnection con = Conexion.Abrir())
            {
                SqliteCommand cmd = con.CreateCommand();
                cmd.CommandText = "UPDATE Reservas SET Estado = 'Cancelada' WHERE Id = @id";
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
            }
        }

        private Reserva Leer(SqliteDataReader dr)
        {
            Reserva r = new Reserva();
            r.Id = dr.GetInt32(0);
            r.ClienteId = dr.GetInt32(1);
            r.PuestoId = dr.GetInt32(2);
            r.FechaInicio = Conexion.TextoAFecha(dr.GetString(3));
            r.FechaFin = Conexion.TextoAFecha(dr.GetString(4));
            r.Estado = (EstadoReserva)Enum.Parse(typeof(EstadoReserva), dr.GetString(5));
            r.CostoTotal = Convert.ToDecimal(dr.GetValue(6));
            return r;
        }
    }
}
