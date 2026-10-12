using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Alquiler_de_Canchas.Models
{
    [Table("imagen")]
    public class Imagen
    {
        [Key, Column("ID_imagen")]
        public int IdImagen {get; set;}
        [Column("ID_cancha")]
		public int IdCancha { get; set; }
        [ForeignKey(nameof(IdCancha))]
        public Cancha? Cancha { get; set; }
		public string Url { get; set; } = "";

        [Column("fecha_subida")]
        public DateTime FechaSubida {get; set;} = DateTime.Now;
        [NotMapped]
		public IFormFile? Archivo { get; set; } = null;
    }

}