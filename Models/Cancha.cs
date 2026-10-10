using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Alquiler_de_Canchas.Models
{
    [Table("cancha")]
    public class Cancha
    {
        [Key, Column("ID_cancha")]
        public int IdCancha {get; set;}
        [Required(ErrorMessage ="El número de cancha es obligatorio")]
        [Range(0, int.MaxValue, ErrorMessage = "El numero debe ser mayor o igual a 0.")]
        public int Numero {get; set;}
        [Required(ErrorMessage = "El tipo de superficie es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El tipo de superficie debe tener entre 3 y 50 caracteres.")]
        public required string TipoSuperficie {get; set;}
        public bool Techada {get; set;} = false;
        [Required(ErrorMessage ="El precio por hora es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio por hora debe ser mayor a 0")]
        public double PrecioPorHora {get; set;}
        [Required(ErrorMessage = "El tipo de cancha es obligatorio")]
        [Column("ID_tipo_cancha")]
        public int IdTipoCancha {get; set;}
        public bool Estado {get; set;} = true;
        [ForeignKey(nameof(IdTipoCancha))]
        public TipoCancha? TipoCancha {get; set;}
    }
}