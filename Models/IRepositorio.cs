// Models/IRepositorio.cs
namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorio<T>
    {
        int Alta(T p);
        int Baja(int id);
        int Modificacion(T p);

        IList<T> ObtenerLista(int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidad();
        T? ObtenerPorId(int id);
    }
}