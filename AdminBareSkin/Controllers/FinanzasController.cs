
using AdminBareSkin.Data;
using AdminBareSkin.Models;
using AdminBareSkin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminBareSkin.Controllers
{
    [Authorize(Roles = "SuperAdmin,Dueño")]
    public class FinanzasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CierreMensualService _cierreService;

        private static readonly string[] Categorias =
        {
            "Publicidad",
            "Transporte",
            "Servicios",
            "Empaque",
            "Comisiones",
            "Mantenimiento",
            "Otros"
        };

        public FinanzasController(
            ApplicationDbContext context,
            CierreMensualService cierreService)
        {
            _context = context;
            _cierreService = cierreService;
        }

        // ==========================================
        // PANEL PRINCIPAL DE FINANZAS
        // ==========================================

        public async Task<IActionResult> Index()
        {
            await AsegurarCuentasAsync();

            var ahoraCostaRica = DateTime.UtcNow.AddHours(-6);

            var inicioMes = new DateTime(
                ahoraCostaRica.Year,
                ahoraCostaRica.Month,
                1, 6, 0, 0, DateTimeKind.Utc);

            var finMes = inicioMes.AddMonths(1);

            var movimientosMes = await _context.MovimientosFinancieros
                .Include(m => m.CuentaFinanciera)
                .Where(m => m.FechaRegistro >= inicioMes &&
                            m.FechaRegistro < finMes)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            var cuentas = await _context.CuentasFinancieras
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            var saldos = new Dictionary<int, decimal>();

            foreach (var cuenta in cuentas)
            {
                var ingresos = await _context.MovimientosFinancieros
                    .Where(m => m.CuentaFinancieraId == cuenta.Id &&
                                m.Tipo == "Ingreso")
                    .SumAsync(m => (decimal?)m.Monto) ?? 0m;

                var egresos = await _context.MovimientosFinancieros
                    .Where(m => m.CuentaFinancieraId == cuenta.Id &&
                                m.Tipo == "Egreso")
                    .SumAsync(m => (decimal?)m.Monto) ?? 0m;

                saldos[cuenta.Id] = ingresos - egresos;
            }

            var ventasMes = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.FechaRegistro >= inicioMes &&
                            v.FechaRegistro < finMes)
                .ToListAsync();

            var totalVentas = ventasMes.Sum(v => v.Total);

            var costoProductos = ventasMes
                .SelectMany(v => v.Detalles)
                .Sum(d => d.PrecioCompraUnitario * d.Cantidad);

            var gananciaBruta = totalVentas - costoProductos;

            var gastosOperativos = movimientosMes
                .Where(m => m.Tipo == "Egreso" && m.EsGastoOperativo)
                .Sum(m => m.Monto);

            ViewBag.Cuentas = cuentas;
            ViewBag.Saldos = saldos;
            ViewBag.TotalVentas = totalVentas;
            ViewBag.CostoProductos = costoProductos;
            ViewBag.GananciaBruta = gananciaBruta;
            ViewBag.GastosOperativos = gastosOperativos;
            ViewBag.GananciaNeta = gananciaBruta - gastosOperativos;

            ViewBag.IngresosMes = movimientosMes
                .Where(m => m.Tipo == "Ingreso")
                .Sum(m => m.Monto);

            ViewBag.EgresosMes = movimientosMes
                .Where(m => m.Tipo == "Egreso")
                .Sum(m => m.Monto);

            ViewBag.Movimientos = movimientosMes;
            ViewBag.MesActual = ahoraCostaRica.ToString("MMMM yyyy");

            if (gananciaBruta - gastosOperativos < 0)
            {
                ViewBag.Recomendacion =
                    "Atención: los gastos operativos superan la ganancia bruta del mes.";
            }
            else if (gastosOperativos > gananciaBruta * 0.5m)
            {
                ViewBag.Recomendacion =
                    "Precaución: los gastos operativos representan más del 50% de la ganancia bruta.";
            }
            else if (totalVentas == 0)
            {
                ViewBag.Recomendacion =
                    "Todavía no hay ventas registradas en este mes.";
            }
            else
            {
                ViewBag.Recomendacion =
                    "El negocio presenta una rentabilidad operativa positiva. Seguí controlando los gastos.";
            }

            return View();
        }

        // ==========================================
        // FORMULARIO PARA REGISTRAR GASTOS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> CrearGasto()
        {
            await AsegurarCuentasAsync();

            ViewBag.Cuentas = await _context.CuentasFinancieras
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Categorias = Categorias;

            return View(new CrearGastoViewModel());
        }

        // ==========================================
        // REGISTRAR Y AUTORIZAR GASTO
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearGasto(CrearGastoViewModel modelo)
        {
            await AsegurarCuentasAsync();

            ViewBag.Cuentas = await _context.CuentasFinancieras
                .Where(c => c.Activa)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Categorias = Categorias;

            if (!Categorias.Contains(modelo.Categoria))
            {
                ModelState.AddModelError(
                    nameof(modelo.Categoria),
                    "Seleccioná una categoría válida.");
            }

            if (modelo.TipoGasto != "Operativo" &&
                modelo.TipoGasto != "Inventario")
            {
                ModelState.AddModelError(
                    nameof(modelo.TipoGasto),
                    "Seleccioná un tipo de gasto válido.");
            }

            var cuenta = await _context.CuentasFinancieras
                .FirstOrDefaultAsync(c =>
                    c.Id == modelo.CuentaFinancieraId && c.Activa);

            if (cuenta == null)
            {
                ModelState.AddModelError(
                    nameof(modelo.CuentaFinancieraId),
                    "La cuenta seleccionada no está disponible.");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            var fechaLocal = DateTime.UtcNow.AddHours(-6);

            if (await PeriodoCerradoAsync(fechaLocal))
            {
                ModelState.AddModelError(
                    "",
                    "El mes actual está cerrado. Reabrilo antes de registrar movimientos.");

                return View(modelo);
            }

            var monto = modelo.Monto;

            var ingresos = await _context.MovimientosFinancieros
                .Where(m => m.CuentaFinancieraId == cuenta!.Id &&
                            m.Tipo == "Ingreso")
                .SumAsync(m => (decimal?)m.Monto) ?? 0m;

            var egresos = await _context.MovimientosFinancieros
                .Where(m => m.CuentaFinancieraId == cuenta!.Id &&
                            m.Tipo == "Egreso")
                .SumAsync(m => (decimal?)m.Monto) ?? 0m;

            if (ingresos - egresos < monto)
            {
                ModelState.AddModelError(
                    "",
                    "La cuenta no tiene saldo suficiente para registrar este gasto.");

                return View(modelo);
            }

            var usuario = User.Identity?.Name ?? "Usuario";

            var movimiento = new MovimientoFinanciero
            {
                CuentaFinancieraId = cuenta!.Id,
                Tipo = "Egreso",
                Categoria = modelo.Categoria,
                Concepto = modelo.Concepto.Trim(),
                Descripcion = modelo.Descripcion.Trim(),
                Monto = monto,
                FechaRegistro = DateTime.UtcNow,
                RegistradoPor = usuario,
                AutorizadoPor = usuario,
                EsGastoOperativo = modelo.TipoGasto == "Operativo",
                EsCompraInventario = modelo.TipoGasto == "Inventario"
            };

            _context.MovimientosFinancieros.Add(movimiento);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] =
                "Gasto registrado y autorizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // REPORTE FINANCIERO ANUAL
        // ==========================================

        public async Task<IActionResult> Reporte(int? anio)
        {
            var ahoraCostaRica = DateTime.UtcNow.AddHours(-6);
            var anioSeleccionado = anio ?? ahoraCostaRica.Year;

            if (anioSeleccionado < 2020 ||
                anioSeleccionado > ahoraCostaRica.Year)
            {
                anioSeleccionado = ahoraCostaRica.Year;
            }

            // Costa Rica utiliza UTC-6.
            // El año local comienza a las 06:00 UTC.
            var inicio = new DateTime(
                anioSeleccionado,
                1,
                1,
                6,
                0,
                0,
                DateTimeKind.Utc);

            var fin = inicio.AddYears(1);

            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .Where(v => v.FechaRegistro >= inicio &&
                            v.FechaRegistro < fin)
                .ToListAsync();

            var movimientos = await _context.MovimientosFinancieros
                .Include(m => m.CuentaFinanciera)
                .Where(m => m.FechaRegistro >= inicio &&
                            m.FechaRegistro < fin)
                .ToListAsync();

            var reporte = new ReporteFinancieroViewModel
            {
                Anio = anioSeleccionado,

                TotalVentas = ventas.Sum(v => v.Total),

                CostoProductos = ventas
                    .SelectMany(v => v.Detalles)
                    .Sum(d => d.PrecioCompraUnitario * d.Cantidad),

                CantidadVentas = ventas.Count,

                UnidadesVendidas = ventas
                    .SelectMany(v => v.Detalles)
                    .Sum(d => d.Cantidad),

                TotalIngresos = movimientos
                    .Where(m => m.Tipo == "Ingreso")
                    .Sum(m => m.Monto),

                TotalEgresos = movimientos
                    .Where(m => m.Tipo == "Egreso")
                    .Sum(m => m.Monto),

                GastosOperativos = movimientos
                    .Where(m => m.Tipo == "Egreso" && m.EsGastoOperativo)
                    .Sum(m => m.Monto),

                ComprasInventario = movimientos
                    .Where(m => m.Tipo == "Egreso" && m.EsCompraInventario)
                    .Sum(m => m.Monto)
            };

            reporte.GananciaBruta =
                reporte.TotalVentas - reporte.CostoProductos;

            reporte.GananciaNeta =
                reporte.GananciaBruta - reporte.GastosOperativos;

            string[] nombresMeses =
            {
                "Enero", "Febrero", "Marzo", "Abril",
                "Mayo", "Junio", "Julio", "Agosto",
                "Septiembre", "Octubre", "Noviembre", "Diciembre"
            };

            for (int mes = 1; mes <= 12; mes++)
            {
                var ventasMes = ventas
                    .Where(v => v.FechaRegistro.AddHours(-6).Month == mes)
                    .ToList();

                var movimientosMes = movimientos
                    .Where(m => m.FechaRegistro.AddHours(-6).Month == mes)
                    .ToList();

                var totalVentasMes = ventasMes.Sum(v => v.Total);

                var costosMes = ventasMes
                    .SelectMany(v => v.Detalles)
                    .Sum(d => d.PrecioCompraUnitario * d.Cantidad);

                var gastosMes = movimientosMes
                    .Where(m => m.Tipo == "Egreso" && m.EsGastoOperativo)
                    .Sum(m => m.Monto);

                reporte.Meses.Add(new ReporteMesViewModel
                {
                    Mes = mes,
                    NombreMes = nombresMeses[mes - 1],
                    Ventas = totalVentasMes,
                    Costos = costosMes,
                    GananciaBruta = totalVentasMes - costosMes,
                    GastosOperativos = gastosMes,
                    GananciaNeta = totalVentasMes - costosMes - gastosMes,
                    CantidadVentas = ventasMes.Count
                });
            }

            reporte.Cuentas = movimientos
                .GroupBy(m => m.CuentaFinanciera?.Nombre ?? "Sin cuenta")
                .Select(grupo => new ReporteCuentaViewModel
                {
                    Nombre = grupo.Key,

                    Ingresos = grupo
                        .Where(m => m.Tipo == "Ingreso")
                        .Sum(m => m.Monto),

                    Egresos = grupo
                        .Where(m => m.Tipo == "Egreso")
                        .Sum(m => m.Monto),

                    Saldo = grupo
                        .Where(m => m.Tipo == "Ingreso")
                        .Sum(m => m.Monto)
                        -
                        grupo.Where(m => m.Tipo == "Egreso")
                            .Sum(m => m.Monto)
                })
                .OrderBy(c => c.Nombre)
                .ToList();

            return View(reporte);
        }

        // ==========================================
        // HISTORIAL DE MOVIMIENTOS
        // ==========================================

        public async Task<IActionResult> Historial()
        {
            var movimientos = await _context.MovimientosFinancieros
                .Include(m => m.CuentaFinanciera)
                .OrderByDescending(m => m.FechaRegistro)
                .ToListAsync();

            return View(movimientos);
        }

        // ==========================================
        // LISTADO DE CIERRES
        // ==========================================

        public async Task<IActionResult> Cierres()
        {
            var cierres = await _context.CierresMensuales
                .Include(c => c.Historial)
                .OrderByDescending(c => c.Anio)
                .ThenByDescending(c => c.Mes)
                .ThenByDescending(c => c.Version)
                .ToListAsync();

            return View(cierres);
        }

        // ==========================================
        // CIERRE MANUAL DE UN MES FINALIZADO
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarMes(int anio, int mes)
        {
            var ahora = DateTime.UtcNow.AddHours(-6);

            if (anio > ahora.Year ||
                (anio == ahora.Year && mes >= ahora.Month) ||
                mes < 1 || mes > 12)
            {
                TempData["Error"] =
                    "Solo podés cerrar meses que ya hayan finalizado.";

                return RedirectToAction(nameof(Cierres));
            }

            try
            {
                await _cierreService.CerrarMesAsync(
                    anio,
                    mes,
                    User.Identity?.Name,
                    false);

                TempData["Mensaje"] =
                    "El mes se cerró correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Cierres));
        }

        // ==========================================
        // REABRIR MES
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReabrirMes(int id, string motivo)
        {
            try
            {
                await _cierreService.ReabrirMesAsync(
                    id,
                    motivo,
                    User.Identity?.Name);

                TempData["Mensaje"] =
                    "El mes fue reabierto. La operación quedó registrada en el historial.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Cierres));
        }

        // ==========================================
        // VALIDAR SI EL PERÍODO ESTÁ CERRADO
        // ==========================================

        private async Task<bool> PeriodoCerradoAsync(DateTime fechaLocal)
        {
            var ultimoCierre = await _context.CierresMensuales
                .Where(c => c.Anio == fechaLocal.Year &&
                            c.Mes == fechaLocal.Month)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            return ultimoCierre?.Estado == "Cerrado";
        }

        // ==========================================
        // CREAR CUENTAS FINANCIERAS SI NO EXISTEN
        // ==========================================

        private async Task AsegurarCuentasAsync()
        {
            string[] nombres =
            {
                "Efectivo",
                "SINPE Móvil",
                "Tarjeta",
                "Transferencia"
            };

            foreach (var nombre in nombres)
            {
                var existe = await _context.CuentasFinancieras
                    .AnyAsync(c => c.Nombre == nombre);

                if (!existe)
                {
                    _context.CuentasFinancieras.Add(
                        new CuentaFinanciera
                        {
                            Nombre = nombre,
                            Activa = true,
                            FechaRegistro = DateTime.UtcNow
                        });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
