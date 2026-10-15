namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioCliente: IRepositorio<Cliente>
    {
        IList<Cliente> BuscarPorTermino(string termino, int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadPorTermino(string termino);
    }
}