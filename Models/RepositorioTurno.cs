using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioTurno : IRepositorioTurno
    {
        private readonly DataContext contexto;
        public RepositorioTurno(DataContext contexto)
        {
            this.contexto = contexto;
        }
        public int Alta(Turno p)
        {
            contexto.Turnos.Add(p);
            return contexto.SaveChanges();
        }

        public int Baja(int id)
        {
            var turno = contexto.Turnos.Find(id);
            if(turno == null)
            {
                return 0;
            }
            contexto.Turnos.Remove(turno);
            return contexto.SaveChanges();
        }

        public IList<Turno> BuscarTurnosEntreFechas(DateTime fechaDesde, DateTime fechaHasta, int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;
            var desde = fechaDesde.Date;
            var hasta = fechaHasta.Date.AddDays(1);
            return contexto.Turnos
                    .Where(t => t.Fecha >= desde && t.Fecha < hasta)
                    .Include(t => t.Cancha)
                    .Include(t => t.Cliente)
                    .Include(t => t.Usuario)
                    .OrderBy(t => t.Fecha).ThenBy(t => t.HoraInicio).ThenBy(t => t.IdTurno)
                    .Skip((paginaNro - 1) * tamPagina)
                    .Take(tamPagina)
                    .ToList();
        }

        public IList<Turno> BuscarTurnosPorCliente(int idCliente, int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;
            return contexto.Turnos
                    .Where(t => t.IdCliente == idCliente)
                    .Include(t => t.Cancha)
                    .Include(t => t.Cliente)
                    .Include(t => t.Usuario)
                    .OrderByDescending(t => t.Fecha).ThenBy(t => t.HoraInicio).ThenBy(t => t.IdTurno)
                    .Skip((paginaNro - 1) * tamPagina)
                    .Take(tamPagina)
                    .ToList();
        }

        public IList<Turno> BuscarTurnosPorEstado(EstadoTurno estado, int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;
            return contexto.Turnos
                    .Where(t => t.Estado == estado)
                    .Include(t => t.Cancha)
                    .Include(t => t.Cliente)
                    .Include(t => t.Usuario)
                    .OrderBy(t => t.Fecha).ThenBy(t => t.HoraInicio).ThenBy(t => t.IdTurno)
                    .Skip((paginaNro - 1) * tamPagina)
                    .Take(tamPagina)
                    .ToList();
        }

        public IList<Turno> BuscarTurnosPorFecha(DateTime fecha, int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;

            var dia = fecha.Date;
            return contexto.Turnos
                .AsNoTracking()
                .Include(t => t.Cancha)
                .Include(t => t.Cliente)
                .Include(t => t.Usuario)
                .Where(t => t.Fecha == dia)
                .OrderBy(t => t.HoraInicio).ThenBy(t => t.IdTurno)
                .Skip((paginaNro - 1) * tamPagina)
                .Take(tamPagina)
                .ToList();
                }

        public int Modificacion(Turno p)
        {
            contexto.Turnos.Update(p);
            return contexto.SaveChanges();
        }

        public int ObtenerCantidad()
        {
            return contexto.Turnos.Count();
        }

        public int ObtenerCantidadEntreFechas(DateTime fechaDesde, DateTime fechaHasta)
        {
            var desde = fechaDesde.Date;
            var hasta = fechaHasta.Date.AddDays(1);
            return contexto.Turnos.Count(t => t.Fecha >= desde && t.Fecha < hasta);
        }

        public int ObtenerCantidadPorCliente(int idCliente)
        {
            return contexto.Turnos.Count(t => t.IdCliente == idCliente);
        }

        public int ObtenerCantidadPorEstado(EstadoTurno estado)
        {
            return contexto.Turnos.Count(t => t.Estado == estado);
        }

        public int ObtenerCantidadPorFecha(DateTime fecha)
        {
            var dia = fecha.Date;
            return contexto.Turnos.Count(t => t.Fecha == dia);
        }

        public IList<Turno> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;
            return contexto.Turnos    
                    .Include(t => t.Cancha)
                    .Include(t => t.Cliente)
                    .Include(t => t.Usuario)
                    .OrderByDescending(t => t.Fecha)
                    .ThenBy(t => t.HoraInicio)
                    .ThenBy(t => t.IdTurno)
                    .Skip((paginaNro - 1) * tamPagina)
                    .Take(tamPagina)
                    .ToList();
        }

        public Turno? ObtenerPorId(int id)
        {
            return contexto.Turnos.FirstOrDefault(t => t.IdTurno == id);
        }
    }
}