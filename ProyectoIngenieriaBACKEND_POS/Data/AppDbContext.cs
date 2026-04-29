using Microsoft.EntityFrameworkCore;
using ProyectoIngenieriaBACKEND_POS.Models.Entities;

namespace ProyectoIngenieriaBACKEND_POS.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Client> Clients { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}