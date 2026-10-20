using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioPago : IRepositorioPago
    {
        private readonly DataContext contexto;

        public RepositorioPago(DataContext contexto)
        {
            this.contexto = contexto;
        }

        public int Alta(Pago p)
        {
            contexto.Pagos.Add(p);
            return contexto.SaveChanges();
        }

        // La tabla pago no tiene campo estado (según el DER): la baja es física.
        // Restringirla al rol Administrador en el controlador.
        public int Baja(int id)
        {
            var pago = contexto.Pagos.Find(id);
            if (pago == null) return 0;
            contexto.Pagos.Remove(pago);
            return contexto.SaveChanges();
        }

        public int Modificacion(Pago p)
        {
            contexto.Pagos.Update(p);
            return contexto.SaveChanges();
        }

        public int ObtenerCantidad()
        {
            return contexto.Pagos.Count();
        }

        // Paginado por servidor: solo trae la página pedida
        public IList<Pago> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;

            return contexto.Pagos
                .AsNoTracking()
                .Include(p => p.Turno)
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.IdPago)
                .Skip((paginaNro - 1) * tamPagina)
                .Take(tamPagina)
                .ToList();
        }

        public Pago? ObtenerPorId(int id)
        {
            return contexto.Pagos
                .AsNoTracking()
                .Include(p => p.Turno)
                .Include(p => p.Usuario)
                .FirstOrDefault(p => p.IdPago == id);
        }

        public IList<Pago> ObtenerPorTurno(int idTurno)
        {
            return contexto.Pagos
                .AsNoTracking()
                .Include(p => p.Usuario)
                .Where(p => p.IdTurno == idTurno)
                .OrderBy(p => p.FechaPago)
                .ToList();
        }
    }
}