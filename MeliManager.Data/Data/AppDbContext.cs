using Microsoft.EntityFrameworkCore;
using MeliManager.Core.Models;

namespace MeliManager.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<CuentaMeli> Cuentas { get; set; }
    }
}