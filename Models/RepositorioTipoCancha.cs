
using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioTipoCancha : IRepositorioTipoCancha
    {
        private readonly DataContext contexto;

        public RepositorioTipoCancha(DataContext contexto)
        {
            this.contexto = contexto;
        }
        public int Alta(TipoCancha p)
        {
            contexto.TipoCanchas.Add(p);
            return contexto.SaveChanges();
        }

        public int Baja(int id)
        {
            var tipoCancha = contexto.TipoCanchas.Find(id);
            if (tipoCancha == null)
            {
                return 0;
            } 
            tipoCancha.Estado = false;
            return contexto.SaveChanges();
        }

        public int Modificacion(TipoCancha p)
        {
            contexto.TipoCanchas.Update(p);
            return contexto.SaveChanges();
        }

        public int ObtenerCantidad()
        {
            return contexto.TipoCanchas.Count();
        }

        public IList<TipoCancha> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;

            return contexto.TipoCanchas
                .AsNoTracking()
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTipoCancha)
                .Skip((paginaNro - 1) * tamPagina)
                .Take(tamPagina)
                .ToList();
        }

        public TipoCancha? ObtenerPorId(int id)
        {
            return contexto.TipoCanchas
                .AsNoTracking()
                .FirstOrDefault(t => t.IdTipoCancha == id);
        }
    }
}