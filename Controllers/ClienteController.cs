
using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Alquiler_de_Canchas.Controllers
{
    [Authorize]
    public class ClienteController : Controller
    {
        
        private readonly IRepositorioCliente repositorio;
        private readonly ILogger<ClienteController> logger;

        public ClienteController(IRepositorioCliente repositorio, ILogger<ClienteController> logger)
        {
            this.repositorio = repositorio;
            this.logger = logger;
        }

         // GET: Cliente
        [AllowAnonymous]
        public IActionResult Index(int pagina = 1)
        {
             try
                {
                    int tamPagina = 10;
                    pagina = Math.Max(pagina, 1);

                    var lista = repositorio.ObtenerLista(pagina, tamPagina);
                    int totalRegistros = repositorio.ObtenerCantidad();
                    int totalPaginas = totalRegistros == 0
                        ? 1
                        : (totalRegistros % tamPagina == 0 ? totalRegistros / tamPagina : totalRegistros / tamPagina + 1);

                    ViewBag.PaginaActual = pagina;
                    ViewBag.TotalPaginas = totalPaginas;

                    return View(lista);
                }
                catch (Exception)
                {
                    TempData["error"] = "No se pudo cargar el listado. Intente nuevamente.";
                    return View(new List<Cliente>());
                }
        }
        public IActionResult Details(int id)
        {
            try
            {
                var cliente = repositorio.ObtenerPorId(id);
                if (cliente == null) return NotFound();
                return View(cliente);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el detalle del cliente (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el cliente. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }
         // GET: Cliente/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Cliente/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                
                repositorio.Alta(cliente);
                TempData["success"] = "Cliente creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al guardar: " + ex.Message);
                return View(cliente);
            }
        }
        // GET: Cliente/Edit
        public IActionResult Edit(int id)
        {
            try
            {
                var cliente = repositorio.ObtenerPorId(id);
                if (cliente == null)
                {
                    return NotFound();
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el cliente para editar (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el cliente. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Cliente cliente)
        {
            if (!ModelState.IsValid)
            {
                return View(cliente);
            }

            try
            {
                cliente.IdCliente = id;
                
                int filasAfectadas = repositorio.Modificacion(cliente);
                if (filasAfectadas > 0)
                {
                    TempData["success"] = "Cliente modificado exitosamente";
                }
                else
                {
                    TempData["error"] = "No se pudo modificar el cliente. Verificá que exista.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                return View(cliente);
            }
        }
        
        // GET: Cliente/Delete
        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            try
            {
                var cliente = repositorio.ObtenerPorId(id);
                if (cliente == null)
                {
                    return NotFound();
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el cliente para eliminar (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el cliente. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Cliente/Delete
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                repositorio.Baja(id);
                TempData["success"] = "Cliente borrado exitosamente";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar el cliente (Id: {Id})", id);
                TempData["error"] = "No se pudo eliminar el cliente. Intente nuevamente.";
            }
            
                return RedirectToAction(nameof(Index));
        }

           
    }
}