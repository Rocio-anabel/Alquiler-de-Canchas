using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TipoCancha> TipoCanchas {get; set;}
        public DbSet<Imagen> Imagenes {get; set;}
        public DbSet<Cancha> Canchas {get; set;}
        public DbSet<Turno> Turnos {get; set;}

        public DbSet<Pago> Pagos { get; set; }
        protected override void OnModelCreating(ModelBuilder mb)
        {
            // enum('Empleado','Administrador') en la BD
            mb.Entity<Usuario>().Property(u => u.Rol).HasConversion<string>();
            mb.Entity<Turno>().Property(t => t.Estado).HasConversion<string>();
            mb.Entity<Pago>().Property(p => p.TipoTransaccion).HasConversion<string>();
        }
    }
}