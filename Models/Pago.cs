using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Alquiler_de_Canchas.Models
{
    [Table("pago")]
    public class Pago
    {
        [Key, Column("ID_pago")]
        public int IdPago { get; set; }

        [Required(ErrorMessage = "El turno es obligatorio")]
        [Column("ID_turno")]
        public int IdTurno { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Column("ID_usuario")]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 99999999.99, ErrorMessage = "El monto debe ser mayor a 0")]
        [Column("monto", TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La fecha de pago es obligatoria")]
        [DataType(DataType.Date)]
        [Column("fecha_pago", TypeName = "date")]
        public DateTime FechaPago { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "El método de pago es obligatorio")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "El método de pago debe tener entre 3 y 30 caracteres")]
        [Column("metodo_pago")]
        public required string MetodoPago { get; set; }

        [Column("tipo_transaccion")]
        public TipoTransaccion TipoTransaccion { get; set; } = TipoTransaccion.PagoTotal;

        [ForeignKey(nameof(IdTurno))]
        public Turno? Turno { get; set; }

        [ForeignKey(nameof(IdUsuario))]
        public Usuario? Usuario { get; set; }
    }

    public enum TipoTransaccion
    {
        Sena,
        PagoTotal,
        Reembolso
    }
}