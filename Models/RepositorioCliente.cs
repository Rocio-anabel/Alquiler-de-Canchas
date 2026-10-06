using MySql.Data.MySqlClient;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioCliente : RepositorioBase, IRepositorioCliente
    {
        public RepositorioCliente(IConfiguration configuration) : base(configuration)
        {
        }

        public int Alta(Cliente p)
        {
            int id = 0;
            var sql = @"INSERT INTO cliente (nombre, apellido, email, telefono) 
                        VALUES (@nombre, @apellido, @email, @telefono);
                        SELECT LAST_INSERT_ID();";
            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombre", p.Nombre);
                command.Parameters.AddWithValue("@apellido", p.Apellido);
                command.Parameters.AddWithValue("@telefono", p.Telefono);
                command.Parameters.AddWithValue("@email", p.Email);

                connection.Open();
                id = Convert.ToInt32(command.ExecuteScalar());
            }

            return id;
        }

        public int Baja(int id)
        {
            int filasAfectadas = 0;
            var sql = @"DELETE FROM cliente WHERE ID_cliente = @id;";
            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }
            return filasAfectadas;

        }

        public int Modificacion(Cliente p)
        {
            int filasAfectadas = 0;
            string sql = @"UPDATE cliente SET
                            nombre = @nombre,
                            apellido = @apellido,
                            telefono = @telefono,
                            email = @email
                            WHERE ID_cliente = @id;";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@nombre", p.Nombre);
                command.Parameters.AddWithValue("@apellido", p.Apellido);
                command.Parameters.AddWithValue("@telefono", p.Telefono);
                command.Parameters.AddWithValue("@email", p.Email);
                command.Parameters.AddWithValue("@id", p.IdCliente);

                connection.Open();
                filasAfectadas = command.ExecuteNonQuery();
            }

            return filasAfectadas;
        }

        public int ObtenerCantidad()
        {
            int cantidad = 0;
            string sql = "SELECT COUNT(*) FROM cliente";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(sql, connection);
                connection.Open();
                cantidad = Convert.ToInt32(command.ExecuteScalar());
            }

            return cantidad;
        }

        public IList<Cliente> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            var lista = new List<Cliente>();
            string sql = @"SELECT ID_cliente, nombre, apellido, telefono, email
                            FROM cliente
                            ORDER BY ID_cliente
                            LIMIT @tamPagina OFFSET @offset;";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@tamPagina", tamPagina);
                command.Parameters.AddWithValue("@offset", (paginaNro - 1) * tamPagina);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Cliente
                        {
                            IdCliente = reader.GetInt32("ID_cliente"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Telefono = reader.GetString("telefono"),
                            Email = reader.GetString("email")
                        });
                    }
                }
            }

            return lista;
        }

        public Cliente? ObtenerPorId(int id)
        {
            Cliente? cliente = null;
            string sql = @"SELECT ID_cliente, nombre, apellido, telefono, email
                            FROM cliente
                            WHERE ID_cliente = @id";

            using (var connection = new MySqlConnection(connectionString))
            {
                var command = new MySqlCommand(sql, connection);
                command.Parameters.AddWithValue("@id", id);

                connection.Open();
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        cliente = new Cliente
                        {
                            IdCliente = reader.GetInt32("ID_cliente"),
                            Nombre = reader.GetString("nombre"),
                            Apellido = reader.GetString("apellido"),
                            Telefono = reader.GetString("telefono"),
                            Email = reader.GetString("email")
                        };
                    }
                }
            }

            return cliente;
        }
    }
}