using System.Globalization;
using System.Security.Claims;
using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alquiler_de_Canchas.Controllers
{
    public class PagoDto
    {
        public int IdPago { get; set; }
        public int IdTurno { get; set; }
        public decimal Monto { get; set; }
        public string FechaPago { get; set; } = "";     // yyyy-MM-dd
        public string MetodoPago { get; set; } = "";
        public int TipoTransaccion { get; set; }
    }

    [Authorize]
    public class PagoController : Controller
    {
        private readonly IRepositorioPago repositorio;
        private readonly IRepositorioTurno repositorioTurno;
        private readonly ILogger<PagoController> logger;

        public PagoController(IRepositorioPago repositorio,
                              IRepositorioTurno repositorioTurno,
                              ILogger<PagoController> logger)
        {
            this.repositorio = repositorio;
            this.repositorioTurno = repositorioTurno;
            this.logger = logger;
        }

        // Solo entrega la página; los datos se piden por AJAX
        public IActionResult Index() => View();

        // ---------- Listado paginado en el servidor (JSON) ----------
        [HttpGet]
        public IActionResult Listar(DateTime? fechaDesde, DateTime? fechaHasta, int? tipo,
                                    int pagina = 1, int tamPagina = 10)
        {
            try
            {
                if (pagina < 1) pagina = 1;
                if (tamPagina < 1) tamPagina = 10;

                TipoTransaccion? tipoFiltro = null;
                if (tipo.HasValue && Enum.IsDefined(typeof(TipoTransaccion), tipo.Value))
                    tipoFiltro = (TipoTransaccion)tipo.Value;

                var lista = repositorio.ObtenerListaFiltrada(fechaDesde, fechaHasta, tipoFiltro, pagina, tamPagina);
                var total = repositorio.ObtenerCantidadFiltrada(fechaDesde, fechaHasta, tipoFiltro);

                var esAdmin = User.IsInRole("Administrador");
                var items = lista.Select(p => new
                {
                    idPago = p.IdPago,
                    idTurno = p.IdTurno,
                    turnoTexto = TextoTurno(p.Turno),
                    monto = p.Monto,
                    fechaPago = p.FechaPago.ToString("yyyy-MM-dd"),
                    metodoPago = p.MetodoPago,
                    tipoTransaccion = (int)p.TipoTransaccion,
                    tipoTexto = TextoTipo(p.TipoTransaccion),
                    usuarioTexto = p.Usuario == null ? "" : $"{p.Usuario.Apellido}, {p.Usuario.Nombre}"
                });

                return Json(new
                {
                    items,
                    total,
                    pagina,
                    totalPaginas = (int)Math.Ceiling(total / (double)tamPagina),
                    esAdmin
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al listar pagos");
                return StatusCode(500, new { mensaje = "No se pudo cargar la lista de pagos." });
            }
        }

        // ---------- Búsqueda asíncrona de turnos (Select2) ----------
        [HttpGet]
        public IActionResult BuscarTurnos(string? term, int pagina = 1)
        {
            const int tam = 10;
            try
            {
                term ??= "";
                var items = repositorioTurno.BuscarPorTermino(term, pagina, tam);
                var total = repositorioTurno.ObtenerCantidadPorTermino(term);
                return Json(new
                {
                    results = items.Select(t => new { id = t.IdTurno, text = TextoTurno(t) }),
                    more = pagina * tam < total
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al buscar turnos");
                return StatusCode(500, new { mensaje = "No se pudo buscar turnos." });
            }
        }

        // ---------- Alta / Modificación / Baja ----------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear([FromBody] PagoDto dto)
        {
            if (!TryConstruir(dto, out var pago, out var error))
                return BadRequest(new { mensaje = error });

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var idUsuario))
                return Unauthorized(new { mensaje = "No se pudo identificar al usuario." });

            pago.IdUsuario = idUsuario;

            try
            {
                if (repositorioTurno.ObtenerPorId(pago.IdTurno) == null)
                    return BadRequest(new { mensaje = "El turno seleccionado no existe." });

                repositorio.Alta(pago);
                return Json(new { mensaje = "Pago registrado correctamente." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al registrar el pago");
                return StatusCode(500, new { mensaje = "No se pudo registrar el pago." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar([FromBody] PagoDto dto)
        {
            if (!TryConstruir(dto, out var datos, out var error))
                return BadRequest(new { mensaje = error });

            try
            {
                var actual = repositorio.ObtenerPorId(dto.IdPago);
                if (actual == null) return NotFound(new { mensaje = "El pago no existe." });

                // Se arma una entidad limpia (solo claves foráneas) para no actualizar Turno/Usuario por error.
                // IdUsuario no se toca: sigue siendo quien registró el pago.
                var pago = new Pago
                {
                    IdPago = actual.IdPago,
                    IdUsuario = actual.IdUsuario,
                    IdTurno = datos.IdTurno,
                    Monto = datos.Monto,
                    FechaPago = datos.FechaPago,
                    MetodoPago = datos.MetodoPago,
                    TipoTransaccion = datos.TipoTransaccion
                };

                repositorio.Modificacion(pago);
                return Json(new { mensaje = "Pago actualizado correctamente." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al editar el pago (Id: {Id})", dto.IdPago);
                return StatusCode(500, new { mensaje = "No se pudo actualizar el pago." });
            }
        }

        // La baja es física (la tabla no tiene estado): solo Administrador
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public IActionResult Eliminar(int id)
        {
            try
            {
                repositorio.Baja(id);
                return Json(new { mensaje = "Pago eliminado correctamente." });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al eliminar el pago (Id: {Id})", id);
                return StatusCode(500, new { mensaje = "No se pudo eliminar el pago." });
            }
        }

        // ---------- Auxiliares ----------
        private static bool TryConstruir(PagoDto dto, out Pago pago, out string error)
        {
            pago = new Pago { MetodoPago = "" };
            error = "";

            if (dto.IdTurno <= 0) { error = "Seleccioná un turno."; return false; }
            if (dto.Monto <= 0 || dto.Monto > 99999999.99m) { error = "El monto debe ser mayor a 0."; return false; }

            if (!DateTime.TryParseExact(dto.FechaPago, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                                        DateTimeStyles.None, out var fecha))
            { error = "La fecha de pago no es válida."; return false; }

            var metodo = (dto.MetodoPago ?? "").Trim();
            if (metodo.Length < 3 || metodo.Length > 30)
            { error = "El método de pago debe tener entre 3 y 30 caracteres."; return false; }

            if (!Enum.IsDefined(typeof(TipoTransaccion), dto.TipoTransaccion))
            { error = "El tipo de transacción no es válido."; return false; }

            pago.IdPago = dto.IdPago;
            pago.IdTurno = dto.IdTurno;
            pago.Monto = Math.Round(dto.Monto, 2);
            pago.FechaPago = fecha;
            pago.MetodoPago = metodo;
            pago.TipoTransaccion = (TipoTransaccion)dto.TipoTransaccion;
            return true;
        }

        private static string TextoTipo(TipoTransaccion t) => t switch
        {
            TipoTransaccion.Sena => "Seña",
            TipoTransaccion.PagoTotal => "Pago total",
            TipoTransaccion.Reembolso => "Reembolso",
            _ => t.ToString()
        };

        private static string TextoTurno(Turno? t)
        {
            if (t == null) return "";
            var cliente = t.Cliente == null ? "" : $"{t.Cliente.Apellido}, {t.Cliente.Nombre}";
            var cancha = t.Cancha == null ? "" : $"Cancha {t.Cancha.Numero}";
            return $"{t.Fecha:dd/MM/yyyy} {t.HoraInicio.ToString("HH:mm")} - {cancha} - {cliente}";
        }
    }
}
