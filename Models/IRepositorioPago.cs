namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioPago : IRepositorio<Pago>
    {
        // Pagos de un turno (útil para el detalle del turno)
        IList<Pago> ObtenerPorTurno(int idTurno);

        // Listado paginado en el servidor con filtros opcionales
        IList<Pago> ObtenerListaFiltrada(DateTime? desde, DateTime? hasta, TipoTransaccion? tipo,
                                         int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadFiltrada(DateTime? desde, DateTime? hasta, TipoTransaccion? tipo);
    }
}
