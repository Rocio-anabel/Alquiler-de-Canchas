using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alquiler_de_Canchas.Controllers
{
    [Authorize]
    public class TipoCanchaController : Controller
    {
        private readonly IRepositorioTipoCancha repositorio;
        private readonly ILogger<TipoCanchaController> logger;

        public TipoCanchaController(IRepositorioTipoCancha repositorio, ILogger<TipoCanchaController> logger)
        {
            this.repositorio = repositorio;
            this.logger = logger;
        }
        
        [AllowAnonymous]
        public IActionResult Index()
        {
            try
            {
                
                var total = repositorio.ObtenerCantidad();
                var lista = repositorio.ObtenerLista(1, total);
                return View(lista);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener la lista de tipos de cancha");
                TempData["error"] = "No se pudo cargar la lista de tipos de cancha. Intente nuevamente.";
                return View(new List<TipoCancha>());
            }
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TipoCancha tipoCancha)
        {
            if (!ModelState.IsValid) return View(tipoCancha);

            try
            {
                repositorio.Alta(tipoCancha);
                TempData["mensaje"] = "Tipo de cancha creado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear el tipo de cancha");
                TempData["error"] = "No se pudo crear el tipo de cancha. Intente nuevamente.";
                return View(tipoCancha);
            }
        }
        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            try
            {
                var tipoCancha = repositorio.ObtenerPorId(id);
                if (tipoCancha == null) return NotFound();
                return View(tipoCancha);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el tipo de cancha para editar (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el tipo de cancha. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, TipoCancha tipoCancha)
        {
            if (id != tipoCancha.IdTipoCancha) return BadRequest();
            if (!ModelState.IsValid) return View(tipoCancha);

            try
            {
                repositorio.Modificacion(tipoCancha);
                TempData["mensaje"] = "Tipo de cancha actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al editar el tipo de cancha (Id: {Id})", tipoCancha.IdTipoCancha);
                TempData["error"] = "No se pudo actualizar el tipo de cancha. Intente nuevamente.";
                return View(tipoCancha);
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            try
            {
                var tipoCancha = repositorio.ObtenerPorId(id);
                if (tipoCancha == null) return NotFound();
                return View(tipoCancha);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el tipo de cancha para eliminar (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el tipo de cancha. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        [Authorize(Roles = "Administrador")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                repositorio.Baja(id);
                TempData["mensaje"] = "Tipo de cancha eliminado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar el tipo de cancha (Id: {Id})", id);
                TempData["error"] = "No se pudo eliminar el tipo de cancha. Puede tener canchas asociadas.";
                return RedirectToAction(nameof(Index));
            }
        }


    }
}