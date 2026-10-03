using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Alquiler_de_Canchas.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly IRepositorioUsuario repositorio;
        private readonly ILogger<UsuarioController> logger;
        private readonly IWebHostEnvironment environment;

        public UsuarioController(IRepositorioUsuario repositorio, ILogger<UsuarioController> logger, IWebHostEnvironment environment)
        {
            this.repositorio = repositorio;
            this.logger = logger;
            this.environment = environment;
        }

        // ---------- LOGIN / LOGOUT ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var usuario = repositorio.ObtenerPorEmail(email);

            if (usuario == null || !usuario.Estado)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            var hasher = new PasswordHasher<Usuario>();
            var resultado = hasher.VerifyHashedPassword(usuario, usuario.Password, password);

            if (resultado == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
                return View();
            }

            await IniciarSesionAsync(usuario);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        public IActionResult AccesoDenegado() => View();

        // ---------- REGISTRO ----------
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Usuario usuario, IFormFile? archivoAvatar)
        {
            if (string.IsNullOrWhiteSpace(usuario.Password))
                ModelState.AddModelError("Password", "La contraseña es obligatoria.");

            if (!ModelState.IsValid)
                return View(usuario);

            try
            {
                usuario.Dni = usuario.Dni.Replace(".", "");

                if (repositorio.ObtenerPorEmail(usuario.Email) != null)
                {
                    ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
                    return View(usuario);
                }
                if (repositorio.ObtenerPorDni(usuario.Dni) != null)
                {
                    ModelState.AddModelError("Dni", "Ya existe un usuario con ese DNI.");
                    return View(usuario);
                }

                var hasher = new PasswordHasher<Usuario>();
                usuario.Password = hasher.HashPassword(usuario, usuario.Password);
                usuario.Rol = RolUsuario.Empleado;   // el registro público siempre crea Empleado
                usuario.Estado = true;

                if (archivoAvatar != null)
                    usuario.Avatar = GuardarAvatar(archivoAvatar);

                repositorio.Alta(usuario);
                TempData["success"] = "Usuario registrado exitosamente. Ya podés iniciar sesión.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar usuario");
                ModelState.AddModelError("", "No se pudo completar el registro. Intente nuevamente.");
                return View(usuario);
            }
        }

        // ---------- ADMINISTRACIÓN (solo Administrador) ----------
        [Authorize(Roles = "Administrador")]
        public IActionResult Index(int pagina = 1)
        {
            try
            {
                int tamPagina = 10;
                pagina = Math.Max(pagina, 1);

                var lista = repositorio.ObtenerLista(pagina, tamPagina);
                int totalRegistros = repositorio.ObtenerCantidad();
                int totalPaginas = totalRegistros == 0 ? 1 : (int)Math.Ceiling(totalRegistros / (double)tamPagina);

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = totalPaginas;
                return View(lista);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el listado de usuarios");
                TempData["error"] = "No se pudo cargar el listado. Intente nuevamente.";
                ViewBag.PaginaActual = 1;
                ViewBag.TotalPaginas = 1;
                return View(new List<Usuario>());
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Details(int id)
        {
            try
            {
                var usuario = repositorio.ObtenerPorId(id);
                if (usuario == null) return NotFound();
                return View(usuario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el detalle del usuario (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el usuario. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Edit(int id)
        {
            try
            {
                var usuario = repositorio.ObtenerPorId(id);
                if (usuario == null) return NotFound();
                return View(usuario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el usuario para editar (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el usuario. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Usuario usuario)
        {
            if (id != usuario.IdUsuario) return NotFound();
            if (!ModelState.IsValid) return View(usuario);

            try
            {
                // Se conserva el hash y el avatar actuales (el admin no los cambia desde acá)
                var actual = repositorio.ObtenerPorId(id);
                if (actual == null) return NotFound();

                usuario.Dni = usuario.Dni.Replace(".", "");
                var otroConDni = repositorio.ObtenerPorDni(usuario.Dni);
                if (otroConDni != null && otroConDni.IdUsuario != id)
                {
                    ModelState.AddModelError("Dni", "Ya existe otro usuario con ese DNI.");
                    return View(usuario);
                }

                usuario.Password = actual.Password;
                usuario.Avatar = actual.Avatar;
                usuario.Email = actual.Email;

                int filas = repositorio.Modificacion(usuario);
                TempData[filas > 0 ? "success" : "error"] =
                    filas > 0 ? "Usuario modificado exitosamente" : "No se pudo modificar el usuario.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al modificar el usuario (Id: {Id})", id);
                ModelState.AddModelError("", "No se pudo guardar los cambios. Intente nuevamente.");
                return View(usuario);
            }
        }

        [Authorize(Roles = "Administrador")]
        public IActionResult Delete(int id)
        {
            int idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (id == idActual)
            {
                TempData["error"] = "No podés eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var usuario = repositorio.ObtenerPorId(id);
                if (usuario == null) return NotFound();
                return View(usuario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el usuario para eliminar (Id: {Id})", id);
                TempData["error"] = "No se pudo cargar el usuario. Intente nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            int idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (id == idActual)
            {
                TempData["error"] = "No podés eliminar tu propio usuario.";
                return RedirectToAction(nameof(Index));
            }
            try
            {
                int filas = repositorio.Baja(id);
                TempData[filas > 0 ? "success" : "error"] =
                    filas > 0 ? "Usuario eliminado exitosamente" : "No se pudo eliminar el usuario.";
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar el usuario (Id: {Id})", id);
                TempData["error"] = "No se pudo eliminar el usuario. Intente nuevamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ---------- MI PERFIL (cualquier usuario logueado) ----------
        [Authorize]
        public IActionResult MiPerfil()
        {
            int idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try
            {
                var usuario = repositorio.ObtenerPorId(idActual);
                if (usuario == null) return NotFound();
                return View(usuario);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al obtener el perfil propio (Id: {Id})", idActual);
                TempData["error"] = "No se pudo cargar tu perfil. Intente nuevamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MiPerfil(Usuario usuarioForm, IFormFile? archivoAvatar, string? passwordActual, string? nuevaPassword)
        {
            int idActual = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (usuarioForm.IdUsuario != idActual) return Forbid();

            try
            {
                var actual = repositorio.ObtenerPorId(idActual);
                if (actual == null) return NotFound();

                var dni = usuarioForm.Dni.Replace(".", "");
                var otroConDni = repositorio.ObtenerPorDni(dni);
                if (otroConDni != null && otroConDni.IdUsuario != idActual)
                {
                    TempData["error"] = "Ya existe otro usuario con ese DNI.";
                    return RedirectToAction(nameof(MiPerfil));
                }

                actual.Nombre = usuarioForm.Nombre;
                actual.Apellido = usuarioForm.Apellido;
                actual.Dni = dni;
                actual.Telefono = usuarioForm.Telefono;

                bool cambiaPassword = !string.IsNullOrWhiteSpace(passwordActual) || !string.IsNullOrWhiteSpace(nuevaPassword);
                if (cambiaPassword)
                {
                    if (string.IsNullOrWhiteSpace(passwordActual) || string.IsNullOrWhiteSpace(nuevaPassword))
                    {
                        ModelState.AddModelError("", "Debe completar ambos campos de contraseña para cambiarla.");
                        return View(actual);
                    }

                    var hasher = new PasswordHasher<Usuario>();
                    if (hasher.VerifyHashedPassword(actual, actual.Password, passwordActual) == PasswordVerificationResult.Failed)
                    {
                        ModelState.AddModelError("", "La contraseña actual es incorrecta.");
                        return View(actual);
                    }
                    if (nuevaPassword.Length < 6)
                    {
                        ModelState.AddModelError("", "La nueva contraseña debe tener al menos 6 caracteres.");
                        return View(actual);
                    }
                    actual.Password = hasher.HashPassword(actual, nuevaPassword);
                }

                if (archivoAvatar != null)
                    actual.Avatar = GuardarAvatar(archivoAvatar);

                repositorio.Modificacion(actual);
                await IniciarSesionAsync(actual);   // refresca los claims (nombre, avatar)

                TempData["success"] = cambiaPassword
                    ? "Perfil y contraseña actualizados exitosamente"
                    : "Perfil actualizado exitosamente";
                return RedirectToAction(nameof(MiPerfil));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al actualizar el perfil propio (Id: {Id})", idActual);
                TempData["error"] = "No se pudo actualizar tu perfil. Intente nuevamente.";
                return RedirectToAction(nameof(MiPerfil));
            }
        }

        // ---------- AUXILIARES ----------
        private async Task IniciarSesionAsync(Usuario usuario)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Surname, usuario.Apellido),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
                new Claim("Avatar", usuario.Avatar ?? "/Uploads/Avatares/avatar-default.png")
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        }

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        private string GuardarAvatar(IFormFile archivo)
        {
            string ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(ext))
                throw new InvalidOperationException("Formato de imagen no permitido.");

            string carpeta = Path.Combine(environment.WebRootPath, "Uploads", "Avatares");
            Directory.CreateDirectory(carpeta);

            string nombre = Guid.NewGuid() + ext;
            using (var stream = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
            {
                archivo.CopyTo(stream);
            }
            return "/Uploads/Avatares/" + nombre;
        }
    }
}