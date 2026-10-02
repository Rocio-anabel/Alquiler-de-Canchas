
using System.ComponentModel.DataAnnotations;

namespace Alquiler_de_Canchas.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email no válido")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 30 caracteres")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessage = "El nombre no puede contener números")]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 30 caracteres")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessage = "El apellido no puede contener números")]
        public required string Apellido { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(@"^(\d{1,3}\.\d{3}\.\d{3}|\d{7,9})$", ErrorMessage = "Formato de DNI inválido")]
        public required string Dni { get; set; }

        public string? Avatar { get; set; }

        [StringLength(20, MinimumLength = 6, ErrorMessage = "El teléfono debe tener entre 6 y 20 caracteres")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        public string? Telefono { get; set; }

        public bool Estado { get; set; } = true;
        public RolUsuario Rol { get; set; }
    }

    public enum RolUsuario
    {
        Empleado,
        Administrador
    }
}