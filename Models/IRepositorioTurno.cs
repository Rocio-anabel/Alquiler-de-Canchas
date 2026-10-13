namespace Alquiler_de_Canchas.Models
{
    public interface IRepositorioTurno: IRepositorio<Turno>
    {
        IList<Turno> BuscarTurnosPorFecha(DateTime fecha);
        IList<Turno> BuscarTurnosEntreFechas(DateTime fechaDesde, DateTime fechaHasta);
        IList<Turno> BuscarTurnosPorCliente(int idCliente);
        IList<Turno> BuscarTurnosPorEstado(EstadoTurno estado);
    }
}