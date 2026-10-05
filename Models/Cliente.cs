using System.ComponentModel.DataAnnotations;
namespace Alquiler_de_Canchas.Models
{
    public class Cliente
    {
        public int IdCliente {get; set;}
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 30 caracteres")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessage = "El nombre no puede contener números")]
        public required string Nombre {get; set;}
        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 30 caracteres")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessage = "El apellido no puede contener números")]
        public required string Apellido {get; set;}
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email no válido")]
        public string? Email {get; set;}
        [Required(ErrorMessage = "El telefono es obligatorio")]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "El teléfono debe tener entre 6 y 20 caracteres")]
        [Phone(ErrorMessage ="Formato de telefono no válido")]
        public string? Telefono {get; set;}

    }
}