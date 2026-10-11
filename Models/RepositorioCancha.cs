using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioCancha : IRepositorioCancha
    {
        private readonly DataContext contexto;

        public RepositorioCancha(DataContext contexto)
        {
            this.contexto = contexto;
        }

        public int Alta(Cancha p)
        {
            contexto.Canchas.Add(p);
            return contexto.SaveChanges();
        }

        // Baja lógica, igual que TipoCancha
        public int Baja(int id)
        {
            var cancha = contexto.Canchas.Find(id);
            if (cancha == null) return 0;
            cancha.Estado = false;
            return contexto.SaveChanges();
        }

        public int Modificacion(Cancha p)
        {
            contexto.Canchas.Update(p);
            return contexto.SaveChanges();
        }

        public int ObtenerCantidad()
        {
            return contexto.Canchas.Count(c => c.Estado);
        }

        // Paginado por servidor: solo trae la página pedida
        public IList<Cancha> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;

            return contexto.Canchas
                .AsNoTracking()
                .Include(c => c.TipoCancha)
                .Include(c => c.Imagenes)
                .Where(c => c.Estado)
                .OrderBy(c => c.Numero)
                .Skip((paginaNro - 1) * tamPagina)
                .Take(tamPagina)
                .ToList();
        }

        public Cancha? ObtenerPorId(int id)
        {
            return contexto.Canchas
                .AsNoTracking()
                .Include(c => c.TipoCancha)
                .Include(c => c.Imagenes)
                .FirstOrDefault(c => c.IdCancha == id);
        }
    }
}