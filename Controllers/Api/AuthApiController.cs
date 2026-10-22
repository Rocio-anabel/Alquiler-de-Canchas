using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Alquiler_de_Canchas.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Alquiler_de_Canchas.Controllers.Api
{
    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly IRepositorioUsuario repositorio;
        private readonly IConfiguration config;
        private readonly ILogger<AuthApiController> logger;

        public AuthApiController(IRepositorioUsuario repositorio,
                                 IConfiguration config,
                                 ILogger<AuthApiController> logger)
        {
            this.repositorio = repositorio;
            this.config = config;
            this.logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest(new { mensaje = "Email y contraseña son obligatorios." });

                var usuario = repositorio.ObtenerPorEmail(req.Email);

                if (usuario == null || !usuario.Estado)
                    return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

                var hasher = new PasswordHasher<Usuario>();
                var resultado = hasher.VerifyHashedPassword(usuario, usuario.Password, req.Password);

                if (resultado == PasswordVerificationResult.Failed)
                    return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Surname, usuario.Apellido),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, usuario.Rol.ToString())
                };

                var jwt = config.GetSection("Jwt");
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
                var expira = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiraMinutos"]!));

                var token = new JwtSecurityToken(
                    issuer: jwt["Issuer"],
                    audience: jwt["Audience"],
                    claims: claims,
                    expires: expira,
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expira,
                    usuario = new
                    {
                        id = usuario.IdUsuario,
                        nombre = usuario.Nombre,
                        apellido = usuario.Apellido,
                        email = usuario.Email,
                        rol = usuario.Rol.ToString()
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en el login de la API");
                return StatusCode(500, new { mensaje = "No se pudo iniciar sesión." });
            }
        }

        
        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Me()
        {
            return Ok(new
            {
                id = User.FindFirstValue(ClaimTypes.NameIdentifier),
                nombre = User.FindFirstValue(ClaimTypes.Name),
                apellido = User.FindFirstValue(ClaimTypes.Surname),
                email = User.FindFirstValue(ClaimTypes.Email),
                rol = User.FindFirstValue(ClaimTypes.Role)
            });
        }
    }
}