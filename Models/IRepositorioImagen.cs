namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioImagen: IRepositorio<Imagen>
    {
        IList<Imagen> BuscarPorCancha(int IdCancha);
    }
}