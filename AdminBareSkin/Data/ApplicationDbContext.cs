
using AdminBareSkin.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdminBareSkin.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }

        // Módulo financiero
        public DbSet<CuentaFinanciera> CuentasFinancieras { get; set; }
        public DbSet<MovimientoFinanciero> MovimientosFinancieros { get; set; }
        public DbSet<CierreMensual> CierresMensuales { get; set; }
        public DbSet<HistorialCierre> HistorialCierres { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Venta>()
                .HasMany(v => v.Detalles)
                .WithOne(d => d.Venta)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MovimientoFinanciero>()
                .HasOne(m => m.CuentaFinanciera)
                .WithMany(c => c.Movimientos)
                .HasForeignKey(m => m.CuentaFinancieraId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MovimientoFinanciero>()
                .HasOne(m => m.Venta)
                .WithMany()
                .HasForeignKey(m => m.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CierreMensual>()
                .HasMany(c => c.Historial)
                .WithOne(h => h.CierreMensual)
                .HasForeignKey(h => h.CierreMensualId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
