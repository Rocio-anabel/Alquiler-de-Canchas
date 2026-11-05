using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Alquiler_de_Canchas.Controllers.Api
{
    [ApiController]
    [Route("api/pagos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PagosApiController : ControllerBase
    {
        private readonly IRepositorioPago repo;
        public PagosApiController(IRepositorioPago repo) => this.repo = repo;

        // GET /api/pagos?pagina=1&tamPagina=10&tipo=1
        [HttpGet]
        public IActionResult Get(DateTime? desde, DateTime? hasta, int? tipo,
                                 int pagina = 1, int tamPagina = 10)
        {
            if (pagina < 1) pagina = 1;
            if (tamPagina < 1 || tamPagina > 100) tamPagina = 10;

            TipoTransaccion? t = tipo.HasValue && Enum.IsDefined(typeof(TipoTransaccion), tipo.Value)
                ? (TipoTransaccion)tipo.Value : null;

            var lista = repo.ObtenerListaFiltrada(desde, hasta, t, pagina, tamPagina);
            var total = repo.ObtenerCantidadFiltrada(desde, hasta, t);

            return Ok(new
            {
                items = lista.Select(p => new
                {
                    p.IdPago,
                    p.IdTurno,
                    p.Monto,
                    fechaPago = p.FechaPago.ToString("yyyy-MM-dd"),
                    p.MetodoPago,
                    tipo = p.TipoTransaccion.ToString()
                }),
                total,
                pagina,
                totalPaginas = (int)Math.Ceiling(total / (double)tamPagina)
            });
        }
    }
}