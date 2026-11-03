
using AdminBareSkin.Data;
using AdminBareSkin.Models;
using Microsoft.EntityFrameworkCore;

namespace AdminBareSkin.Services
{
    public class CierreMensualService
    {
        private readonly ApplicationDbContext _context;

        public CierreMensualService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PeriodoCerradoAsync(DateTime fecha)
        {
            var cierre = await _context.CierresMensuales
                .Where(c => c.Anio == fecha.Year && c.Mes == fecha.Month)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            return cierre?.Estado == "Cerrado";
        }

        public async Task<CierreMensual> CerrarMesAsync(
            int anio,
            int mes,
            string? usuario,
            bool automatico = false)
        {
            if (mes < 1 || mes > 12)
                throw new ArgumentException("El mes no es válido.");

            var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
            var fin = inicio.AddMonths(1);

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var ultimoCierre = await _context.CierresMensuales
                .Where(c => c.Anio == anio && c.Mes == mes)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            if (ultimoCierre?.Estado == "Cerrado")
                throw new InvalidOperationException(
                    "Este mes ya se encuentra cerrado.");

            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.FechaRegistro >= inicio &&
                            v.FechaRegistro < fin)
                .ToListAsync();

            var movimientos = await _context.MovimientosFinancieros
                .Where(m => m.FechaRegistro >= inicio &&
                            m.FechaRegistro < fin)
                .ToListAsync();

            var totalVentas = ventas.Sum(v => v.Total);

            var costoProductos = ventas
                .SelectMany(v => v.Detalles)
                .Sum(d => d.PrecioCompraUnitario * d.Cantidad);

            var gananciaBruta = totalVentas - costoProductos;

            var gastosOperativos = movimientos
                .Where(m => m.Tipo == "Egreso" && m.EsGastoOperativo)
                .Sum(m => m.Monto);

            var comprasInventario = movimientos
                .Where(m => m.Tipo == "Egreso" && m.EsCompraInventario)
                .Sum(m => m.Monto);

            var ingresos = movimientos
                .Where(m => m.Tipo == "Ingreso")
                .Sum(m => m.Monto);

            var egresos = movimientos
                .Where(m => m.Tipo == "Egreso")
                .Sum(m => m.Monto);

            var cierre = new CierreMensual
            {
                Anio = anio,
                Mes = mes,
                Version = (ultimoCierre?.Version ?? 0) + 1,
                Estado = "Cerrado",
                Automatico = automatico,

                TotalVentas = totalVentas,
                CostoProductosVendidos = costoProductos,
                GananciaBruta = gananciaBruta,
                GastosOperativos = gastosOperativos,
                GananciaNetaOperativa = gananciaBruta - gastosOperativos,
                ComprasInventario = comprasInventario,
                IngresosRegistrados = ingresos,
                EgresosRegistrados = egresos,

                CantidadVentas = ventas.Count,
                UnidadesVendidas = ventas
                    .SelectMany(v => v.Detalles)
                    .Sum(d => d.Cantidad),

                FechaDesde = inicio,
                FechaHasta = fin.AddTicks(-1),
                FechaCierre = DateTime.UtcNow,
                CerradoPor = usuario,
                CierreAnteriorId = ultimoCierre?.Id
            };

            _context.CierresMensuales.Add(cierre);
            await _context.SaveChangesAsync();

            _context.HistorialCierres.Add(new HistorialCierre
            {
                CierreMensualId = cierre.Id,
                Accion = automatico ? "Cierre automático" : "Cierre manual",
                Usuario = usuario,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return cierre;
        }

        public async Task ReabrirMesAsync(
            int cierreId,
            string motivo,
            string? usuario)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new ArgumentException("El motivo es obligatorio.");

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            var cierre = await _context.CierresMensuales
                .FirstOrDefaultAsync(c => c.Id == cierreId);

            if (cierre == null)
                throw new InvalidOperationException("No se encontró el cierre.");

            var ultimoCierre = await _context.CierresMensuales
                .Where(c => c.Anio == cierre.Anio && c.Mes == cierre.Mes)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            if (ultimoCierre?.Id != cierre.Id || cierre.Estado != "Cerrado")
                throw new InvalidOperationException(
                    "Solo se puede reabrir el último cierre activo.");

            cierre.Estado = "Reabierto";

            _context.HistorialCierres.Add(new HistorialCierre
            {
                CierreMensualId = cierre.Id,
                Accion = "Reapertura",
                Motivo = motivo.Trim(),
                Usuario = usuario,
                Fecha = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
