using MySql.Data.MySqlClient;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

        private const string Columnas =
            "ID_usuario, email, password, avatar, nombre, apellido, dni, telefono, rol, estado";

        private static Usuario Mapear(MySqlDataReader r) => new Usuario
        {
            IdUsuario = r.GetInt32("ID_usuario"),
            Email = r.GetString("email"),
            Password = r.GetString("password"),
            Avatar = r.IsDBNull(r.GetOrdinal("avatar")) ? null : r.GetString("avatar"),
            Nombre = r.GetString("nombre"),
            Apellido = r.GetString("apellido"),
            Dni = r.GetString("dni"),
            Telefono = r.IsDBNull(r.GetOrdinal("telefono")) ? null : r.GetString("telefono"),
            Rol = Enum.Parse<RolUsuario>(r.GetString("rol")),
            Estado = r.GetBoolean("estado")
        };

        public int Alta(Usuario p)
        {
            var sql = @"INSERT INTO usuario (email, password, avatar, nombre, apellido, dni, telefono, rol, estado)
                        VALUES (@email, @password, @avatar, @nombre, @apellido, @dni, @telefono, @rol, @estado);
                        SELECT LAST_INSERT_ID();";
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@email", p.Email);
            command.Parameters.AddWithValue("@password", p.Password);
            command.Parameters.AddWithValue("@avatar", (object?)p.Avatar ?? DBNull.Value);
            command.Parameters.AddWithValue("@nombre", p.Nombre);
            command.Parameters.AddWithValue("@apellido", p.Apellido);
            command.Parameters.AddWithValue("@dni", p.Dni);
            command.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
            command.Parameters.AddWithValue("@rol", p.Rol.ToString());
            command.Parameters.AddWithValue("@estado", p.Estado);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }

        // Baja lógica
        public int Baja(int id)
        {
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand("UPDATE usuario SET estado = 0 WHERE ID_usuario = @id", connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int Modificacion(Usuario p)
        {
            var sql = @"UPDATE usuario SET avatar=@avatar, nombre=@nombre, apellido=@apellido, dni=@dni,
                               telefono=@telefono, rol=@rol, password=@password
                        WHERE ID_usuario=@id";
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@avatar", (object?)p.Avatar ?? DBNull.Value);
            command.Parameters.AddWithValue("@nombre", p.Nombre);
            command.Parameters.AddWithValue("@apellido", p.Apellido);
            command.Parameters.AddWithValue("@dni", p.Dni);
            command.Parameters.AddWithValue("@telefono", (object?)p.Telefono ?? DBNull.Value);
            command.Parameters.AddWithValue("@rol", p.Rol.ToString());
            command.Parameters.AddWithValue("@password", p.Password);
            command.Parameters.AddWithValue("@id", p.IdUsuario);
            connection.Open();
            return command.ExecuteNonQuery();
        }

        public int ObtenerCantidad()
        {
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand("SELECT COUNT(*) FROM usuario WHERE estado = 1", connection);
            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public IList<Usuario> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            var lista = new List<Usuario>();
            var sql = $@"SELECT {Columnas} FROM usuario WHERE estado = 1
                         ORDER BY ID_usuario LIMIT @tamPagina OFFSET @offset";
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@tamPagina", tamPagina);
            command.Parameters.AddWithValue("@offset", (paginaNro - 1) * tamPagina);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read()) lista.Add(Mapear(reader));
            return lista;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            var sql = $"SELECT {Columnas} FROM usuario WHERE estado = 1 AND email = @email";
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@email", email);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        public Usuario? ObtenerPorDni(string dni)
        {
            var sql = $"SELECT {Columnas} FROM usuario WHERE dni = @dni";
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dni", dni);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }

        public Usuario? ObtenerPorId(int id)
        {
            var sql = $"SELECT {Columnas} FROM usuario WHERE ID_usuario = @id";
            using var connection = new MySqlConnection(connectionString);
            var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? Mapear(reader) : null;
        }
    }
}