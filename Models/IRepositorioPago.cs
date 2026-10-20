namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioPago : IRepositorio<Pago>
    {
        // Pagos de un turno (útil para el detalle del turno)
        IList<Pago> ObtenerPorTurno(int idTurno);
    }
}