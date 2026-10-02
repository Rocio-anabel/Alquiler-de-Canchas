namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioUsuario : IRepositorio<Usuario>
    {
        Usuario? ObtenerPorEmail(string email);
        Usuario? ObtenerPorDni(string dni);
    }
}