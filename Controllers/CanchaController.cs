using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alquiler_de_Canchas.Controllers
{
    [Authorize]
    public class CanchaController : Controller
    {
        private const long TamMaxImagen = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly IRepositorioCancha repositorio;
        private readonly IRepositorioImagen repositorioImagen;
        private readonly IRepositorioTipoCancha repositorioTipo;
        private readonly IWebHostEnvironment environment;
        private readonly ILogger<CanchaController> logger;

        public CanchaController(
            IRepositorioCancha repositorio,
            IRepositorioImagen repositorioImagen,
            IRepositorioTipoCancha repositorioTipo,
            IWebHostEnvironment environment,
            ILogger<CanchaController> logger)
        {
            this.repositorio = repositorio;
            this.repositorioImagen = repositorioImagen;
            this.repositorioTipo = repositorioTipo;
            this.environment = environment;
            this.logger = logger;
        }

        // GET: Cancha  (paginado por servidor)
        public IActionResult Index(int pagina = 1)
        {
            try
            {
                int tamPagina = 10;
                pagina = Math.Max(pagina, 1);

                var lista = repositorio.ObtenerLista(pagina, tamPagina);
                int total = repositorio.ObtenerCantidad();
                int totalPaginas = total == 0 ? 1 : (int)Math.Ceiling(total / (double)tamPagina);

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                return View(lista);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al listar canchas");
                TempData["error"] = "No se pudo cargar la lista de canchas.";
                ViewBag.PaginaActual = 1;
                ViewBag.TotalPaginas = 1;
                return View(new List<Cancha>());
            }
        }

        // GET: Cancha/Details/5
        public IActionResult Details(int id)
        {
            var cancha = repositorio.ObtenerPorId(id);
            if (cancha == null) return NotFound();
            return View(cancha);
        }

        // GET: Cancha/Create
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            CargarTipos();
            return View(new Cancha { TipoSuperficie = "" });
        }

        // POST: Cancha/Create
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cancha cancha, List<IFormFile>? archivos)
        {
            // Propiedades de navegación: no vienen del formulario
            ModelState.Remove(nameof(Cancha.TipoCancha));
            ModelState.Remove(nameof(Cancha.Imagenes));

            if (!ModelState.IsValid)
            {
                CargarTipos(cancha.IdTipoCancha);
                return View(cancha);
            }

            var error = ValidarArchivos(archivos);
            if (error != null)
            {
                ModelState.AddModelError("", error);
                CargarTipos(cancha.IdTipoCancha);
                return View(cancha);
            }

            try
            {
                cancha.Estado = true;
                repositorio.Alta(cancha);            
                GuardarImagenes(cancha.IdCancha, archivos);
                TempData["success"] = "Cancha creada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear la cancha");
                ModelState.AddModelError("", "No se pudo crear la cancha. Intente nuevamente.");
                CargarTipos(cancha.IdTipoCancha);
                return View(cancha);
            }
        }

        // GET: Cancha/Edit/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            var cancha = repositorio.ObtenerPorId(id);
            if (cancha == null) return NotFound();
            CargarTipos(cancha.IdTipoCancha);
            return View(cancha);
        }

        // POST: Cancha/Edit/5
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cancha cancha, List<IFormFile>? archivos)
        {
            ModelState.Remove(nameof(Cancha.TipoCancha));
            ModelState.Remove(nameof(Cancha.Imagenes));
            cancha.IdCancha = id;

            var error = ValidarArchivos(archivos);
            if (error != null) ModelState.AddModelError("", error);

            if (!ModelState.IsValid)
            {
                cancha.Imagenes = repositorioImagen.BuscarPorCancha(id).ToList();
                CargarTipos(cancha.IdTipoCancha);
                return View(cancha);
            }

            try
            {
                cancha.Estado = true;
                repositorio.Modificacion(cancha);
                GuardarImagenes(id, archivos);
                TempData["success"] = "Cancha modificada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al modificar la cancha (Id: {Id})", id);
                ModelState.AddModelError("", "No se pudo modificar la cancha. Intente nuevamente.");
                cancha.Imagenes = repositorioImagen.BuscarPorCancha(id).ToList();
                CargarTipos(cancha.IdTipoCancha);
                return View(cancha);
            }
        }

        // GET: Cancha/Delete/5
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            var cancha = repositorio.ObtenerPorId(id);
            if (cancha == null) return NotFound();
            return View(cancha);
        }

        // POST: Cancha/Delete/5  (baja lógica)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                repositorio.Baja(id);
                TempData["success"] = "Cancha dada de baja correctamente.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al dar de baja la cancha (Id: {Id})", id);
                TempData["error"] = "No se pudo dar de baja la cancha.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Cancha/EliminarImagen  (borra registro + archivo físico)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarImagen(int idImagen, int idCancha)
        {
            try
            {
                var imagen = repositorioImagen.ObtenerPorId(idImagen);
                if (imagen != null && imagen.IdCancha == idCancha)
                {
                    repositorioImagen.Baja(idImagen);
                    BorrarArchivoFisico(imagen.Url);
                    TempData["success"] = "Imagen eliminada.";
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar la imagen (Id: {Id})", idImagen);
                TempData["error"] = "No se pudo eliminar la imagen.";
            }
            return RedirectToAction(nameof(Edit), new { id = idCancha });
        }

        // ------------------------- helpers -------------------------

        private void CargarTipos(int? seleccionado = null)
        {
            // Tabla maestra chica: se puede cargar completa en un <select>
            int total = repositorioTipo.ObtenerCantidad();
            var tipos = repositorioTipo.ObtenerLista(1, Math.Max(total, 1));
            ViewBag.Tipos = new SelectList(tipos, nameof(TipoCancha.IdTipoCancha), nameof(TipoCancha.Nombre), seleccionado);
        }

        private string? ValidarArchivos(List<IFormFile>? archivos)
        {
            if (archivos == null) return null;
            foreach (var f in archivos.Where(f => f.Length > 0))
            {
                string ext = Path.GetExtension(f.FileName).ToLowerInvariant();
                if (!ExtensionesPermitidas.Contains(ext))
                    return $"'{f.FileName}': formato no permitido (solo JPG, PNG o WEBP).";
                if (f.Length > TamMaxImagen)
                    return $"'{f.FileName}': supera el máximo de 5 MB.";
            }
            return null;
        }

        private void GuardarImagenes(int idCancha, List<IFormFile>? archivos)
        {
            if (archivos == null) return;

            string carpeta = Path.Combine(environment.WebRootPath, "Uploads", "Canchas");
            Directory.CreateDirectory(carpeta);

            foreach (var f in archivos.Where(f => f.Length > 0))
            {
                string nombre = Guid.NewGuid() + Path.GetExtension(f.FileName).ToLowerInvariant();
                using (var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
                {
                    f.CopyTo(stream);
                }
                repositorioImagen.Alta(new Imagen
                {
                    IdCancha = idCancha,
                    Url = "/Uploads/Canchas/" + nombre,
                    FechaSubida = DateTime.Now
                });
            }
        }

        private void BorrarArchivoFisico(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            string ruta = Path.Combine(environment.WebRootPath, url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(ruta)) System.IO.File.Delete(ruta);
        }
    }
}