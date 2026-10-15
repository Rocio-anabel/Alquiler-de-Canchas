using System.Globalization;
using System.Security.Claims;
using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alquiler_de_Canchas.Controllers
{
    public class TurnoDto
    {
        public int IdTurno { get; set; }
        public int IdCancha { get; set; }
        public int IdCliente { get; set; }
        public string Fecha { get; set; } = "";        // yyyy-MM-dd
        public string HoraInicio { get; set; } = "";   // HH:mm
        public int Estado { get; set; }
    }

    [Authorize]
    public class TurnoController : Controller
    {
        private const int DuracionMinutos = 90;

        private readonly IRepositorioTurno repositorio;
        private readonly IRepositorioCliente repositorioCliente;
        private readonly IRepositorioCancha repositorioCancha;
        private readonly ILogger<TurnoController> logger;

        public TurnoController(IRepositorioTurno repositorio,
                               IRepositorioCliente repositorioCliente,
                               IRepositorioCancha repositorioCancha,
                               ILogger<TurnoController> logger)
        {
            this.repositorio = repositorio;
            this.repositorioCliente = repositorioCliente;
            this.repositorioCancha = repositorioCancha;
            this.logger = logger;
        }

        // Solo entrega la página; los datos se piden por AJAX
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Listar(DateTime? fechaDesde, DateTime? fechaHasta, int? idCliente,
                                    EstadoTurno? estado, int pagina = 1, int tamPagina = 10)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                if (tamPagina < 1) tamPagina = 10;

                IList<Turno> lista;
                int total;

                if (idCliente.HasValue)
                {
                    lista = repositorio.BuscarTurnosPorCliente(idCliente.Value, pagina, tamPagina);
                    total = repositorio.ObtenerCantidadPorCliente(idCliente.Value);
                }
                else if (estado.HasValue)
                {
                    lista = repositorio.BuscarTurnosPorEstado(estado.Value, pagina, tamPagina);
                    total = repositorio.ObtenerCantidadPorEstado(estado.Value);
                }
                else if (fechaDesde.HasValue && fechaHasta.HasValue)
                {
                    lista = repositorio.BuscarTurnosEntreFechas(fechaDesde.Value, fechaHasta.Value, pagina, tamPagina);
                    total = repositorio.ObtenerCantidadEntreFechas(fechaDesde.Value, fechaHasta.Value);
                }
                else if (fechaDesde.HasValue)
                {
                    lista = repositorio.BuscarTurnosPorFecha(fechaDesde.Value, pagina, tamPagina);
                    total = repositorio.ObtenerCantidadPorFecha(fechaDesde.Value);
                }
                else
                {
                    lista = repositorio.ObtenerLista(pagina, tamPagina);
                    total = repositorio.ObtenerCantidad();
                }

                var esAdmin = User.IsInRole("Administrador");
                var items = lista.Select(t => new
                {
                    idTurno = t.IdTurno,
                    idCancha = t.IdCancha,
                    canchaTexto = TextoCancha(t.Cancha),
                    idCliente = t.IdCliente,
                    clienteTexto = t.Cliente == null ? "" : $"{t.Cliente.Apellido}, {t.Cliente.Nombre}",
                    fecha = t.Fecha.ToString("yyyy-MM-dd"),
                    horaInicio = t.HoraInicio.ToString("HH:mm"),
                    horaFin = t.HoraFin.ToString("HH:mm"),
                    estado = (int)t.Estado,
                    estadoTexto = t.Estado.ToString(),
                    usuarioTexto = esAdmin && t.Usuario != null
                        ? $"{t.Usuario.Apellido}, {t.Usuario.Nombre}"
                        : null
                });

                return Json(new
                {
                    items,
                    total,
                    pagina,
                    totalPaginas = (int)Math.Ceiling(total / (double)tamPagina)
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al listar turnos");
                return StatusCode(500, new { mensaje = "No se pudo cargar la lista de turnos." });
            }
        }

        // ---------- Búsquedas para Select2 ----------

        [HttpGet]
        public IActionResult BuscarClientes(string? term, int pagina = 1)
        {
            const int tam = 10;
            try
            {
                term ??= "";
                var items = repositorioCliente.BuscarPorTermino(term, pagina, tam);
                var total = repositorioCliente.ObtenerCantidadPorTermino(term);
                return Json(new
                {
                    results = items.Select(c => new { id = c.IdCliente, text = $"{c.Apellido}, {c.Nombre}" }),
                    more = pagina * tam < total
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al buscar clientes");
                return StatusCode(500, new { mensaje = "No se pudo buscar clientes." });
            }
        }

        [HttpGet]
        public IActionResult BuscarCanchas(string? term, int pagina = 1)
        {
            const int tam = 10;
            try
            {
                term ??= "";
                var items = repositorioCancha.BuscarPorTermino(term, pagina, tam);
                var total = repositorioCancha.ObtenerCantidadPorTermino(term);
                return Json(new
                {
                    results = items.Select(c => new { id = c.IdCancha, text = TextoCancha(c) }),
                    more = pagina * tam < total
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al buscar canchas");
                return StatusCode(500, new { mensaje = "No se pudo buscar canchas." });
            }
        }

        // ---------- Alta / Modificación / Baja ----------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear([FromBody] TurnoDto dto)
        {
            if (!TryConstruir(dto, out var turno, out var error))
                return BadRequest(new { mensaje = error });

            // Id del usuario logueado (ajustá el claim si guardás el id en otro)
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario))
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            turno.IdUsuario = idUsuario;
            turno.Estado = EstadoTurno.Confirmado;

            try
            {
                repositorio.Alta(turno);
                return Json(new { mensaje = "Turno creado correctamente." });
            }
            catch (InvalidOperationException ex)   // cancha ocupada
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al crear el turno");
                return StatusCode(500, new { mensaje = "No se pudo crear el turno." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar([FromBody] TurnoDto dto)
        {
            if (!TryConstruir(dto, out var datos, out var error))
                return BadRequest(new { mensaje = error });
            if (!Enum.IsDefined(typeof(EstadoTurno), dto.Estado))
                return BadRequest(new { mensaje = "Estado no válido." });

            try
            {
                var turno = repositorio.ObtenerPorId(dto.IdTurno);
                if (turno == null) return NotFound(new { mensaje = "El turno no existe." });

                turno.IdCancha = datos.IdCancha;
                turno.IdCliente = datos.IdCliente;
                turno.Fecha = datos.Fecha;
                turno.HoraInicio = datos.HoraInicio;
                turno.HoraFin = datos.HoraFin;
                turno.Estado = (EstadoTurno)dto.Estado;
                // IdUsuario no se toca: sigue siendo quien lo creó

                repositorio.Modificacion(turno);
                return Json(new { mensaje = "Turno actualizado correctamente." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al editar el turno (Id: {Id})", dto.IdTurno);
                return StatusCode(500, new { mensaje = "No se pudo actualizar el turno." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            try
            {
                repositorio.Baja(id);
                return Json(new { mensaje = "Turno eliminado correctamente." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar el turno (Id: {Id})", id);
                return StatusCode(500, new { mensaje = "No se pudo eliminar el turno." });
            }
        }

        // Valida el DTO y arma el Turno; la hora de fin siempre se calcula (1 h 30)
        private static bool TryConstruir(TurnoDto dto, out Turno turno, out string error)
        {
            turno = new Turno();
            error = "";

            if (dto.IdCancha <= 0) { error = "Seleccioná una cancha."; return false; }
            if (dto.IdCliente <= 0) { error = "Seleccioná un cliente."; return false; }

            if (!DateTime.TryParseExact(dto.Fecha, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var fecha))
            { error = "La fecha no es válida."; return false; }

            if (!TimeOnly.TryParseExact(dto.HoraInicio, "HH:mm", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var inicio))
            { error = "La hora de inicio no es válida."; return false; }

            var fin = inicio.AddMinutes(DuracionMinutos);
            if (fin <= inicio)
            { error = "El turno no puede terminar después de la medianoche."; return false; }

            turno.IdTurno = dto.IdTurno;
            turno.IdCancha = dto.IdCancha;
            turno.IdCliente = dto.IdCliente;
            turno.Fecha = fecha;
            turno.HoraInicio = inicio;
            turno.HoraFin = fin;
            return true;
        }

        private static string TextoCancha(Cancha? c)
        {
            if (c == null) return "";
            var tipo = c.TipoCancha?.Nombre ?? "Sin tipo";
            return $"Cancha {c.Numero} - {tipo} - {c.TipoSuperficie}";
        }
    }
}