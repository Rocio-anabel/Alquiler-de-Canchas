namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioCancha: IRepositorio<Cancha>
    {
        IList<Cancha> BuscarPorTermino(string termino, int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadPorTermino(string termino);
    }
}