
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquiler_de_Canchas.Models
{
    [Table("turno")]
    public class Turno: IValidatableObject
    {
        [Key, Column("ID_turno")]
        public int IdTurno {get; set;}
        [Column("ID_cancha")]
        public int IdCancha {get;set;}
        [Column("ID_cliente")]
        public int IdCliente {get;set;}
        [Column("ID_usuario")]
        public int IdUsuario {get; set;}
        [Required(ErrorMessage = "La fecha del turno es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime Fecha {get; set;}
        [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
        public TimeOnly HoraInicio {get; set;}
        [Required(ErrorMessage = "La hora de fin es obligatoria.")]
        public TimeOnly HoraFin {get; set;}
        public EstadoTurno Estado {get; set;} = EstadoTurno.Confirmado;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HoraFin <= HoraInicio)
            {
                yield return new ValidationResult(
                    "La hora de finalización debe ser posterior a la hora de inicio.",
                    new[] { nameof(HoraFin) } 
                );
            }
        }
    }
    public enum EstadoTurno
        {
            Confirmado,
            Finalizado,
            Cancelado,
            Ausente
        }
    
}