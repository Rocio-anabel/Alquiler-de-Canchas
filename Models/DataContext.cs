using Microsoft.EntityFrameworkCore;

namespace Alquiler_de_Canchas.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // enum('Empleado','Administrador') en la BD
            mb.Entity<Usuario>().Property(u => u.Rol).HasConversion<string>();
        }
    }
}