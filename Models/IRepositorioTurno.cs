namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioTurno: IRepositorio<Turno>
    {
        IList<Turno> BuscarTurnosPorFecha(DateTime fecha, int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadPorFecha(DateTime fecha);
        IList<Turno> BuscarTurnosEntreFechas(DateTime fechaDesde, DateTime fechaHasta, int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadEntreFechas(DateTime fechaDesde, DateTime fechaHasta);
        IList<Turno> BuscarTurnosPorCliente(int idCliente, int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadPorCliente(int idCliente);
        IList<Turno> BuscarTurnosPorEstado(EstadoTurno estado, int paginaNro = 1, int tamPagina = 10);
        int ObtenerCantidadPorEstado(EstadoTurno estado);
    }
}