using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquiler_de_Canchas.Models
{
    [Table("tipo_cancha")]
    public class TipoCancha
    {
        [Key, Column("ID_tipo_cancha")]
        public int IdTipoCancha {get; set;}
        [Required(ErrorMessage = "El campo es obligatorio")]
        [MinLength(2, ErrorMessage = "Mínimo de 2 caracteres")]
        public required string Nombre {get; set;}
        public bool Estado {get; set;} = true;
    }                 
    
}