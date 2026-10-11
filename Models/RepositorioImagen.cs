using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class RepositorioImagen : IRepositorioImagen
    {
         private readonly DataContext contexto;

        public RepositorioImagen(DataContext contexto)
        {
            this.contexto = contexto;
        }
        public int Alta(Imagen p)
        {
            contexto.Imagenes.Add(p);
            return contexto.SaveChanges();
        }

        public int Baja(int id)
        {
            var imagen = contexto.Imagenes.Find(id);
            if(imagen == null)
            {
                return 0;
            }
            contexto.Imagenes.Remove(imagen);
            return contexto.SaveChanges();
        }

        public IList<Imagen> BuscarPorCancha(int IdCancha)
        {
            return contexto.Imagenes
                    .Where(i => i.IdCancha == IdCancha)
                    .OrderBy(i => i.FechaSubida)
                    .ToList();
        }

        public int Modificacion(Imagen p)
        {
            contexto.Update(p);
            return contexto.SaveChanges();
        }

        public int ObtenerCantidad()
        {
            return contexto.Imagenes.Count();
        }

        public IList<Imagen> ObtenerLista(int paginaNro = 1, int tamPagina = 10)
        {
            if (paginaNro < 1) paginaNro = 1;
            if (tamPagina < 1) tamPagina = 10;
            return contexto.Imagenes
                    .AsNoTracking()
                    .OrderBy(i => i.IdImagen)
                    .Skip((paginaNro - 1) * tamPagina)
                    .Take(tamPagina)
                    .ToList();
        }

        public Imagen? ObtenerPorId(int id)
        {
            return contexto.Imagenes
                    .AsNoTracking()
                    .FirstOrDefault(i => i.IdImagen == id);
        }
    }
}