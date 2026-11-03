
using AdminBareSkin.Data;
using AdminBareSkin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminBareSkin.Controllers
{
    [Authorize(Roles = "SuperAdmin,Dueño")]
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        private static readonly string[] MetodosPago =
        {
            "Efectivo", "SINPE Móvil", "Tarjeta", "Transferencia"
        };

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Ventas
                .Include(v => v.Detalles)
                .OrderByDescending(v => v.FechaRegistro)
                .ToListAsync();

            return View(ventas);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarProductos();
            return View(new CrearVentaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearVentaViewModel modelo)
        {
            if (!MetodosPago.Contains(modelo.MetodoPago))
                ModelState.AddModelError(
                    nameof(modelo.MetodoPago),
                    "Seleccioná un método de pago válido.");

            if (modelo.Detalles == null || !modelo.Detalles.Any())
                ModelState.AddModelError(
                    "",
                    "Agregá al menos un producto a la venta.");

            if (modelo.Detalles != null &&
                modelo.Detalles.Any(d => d.ProductoId <= 0 || d.Cantidad <= 0))
                ModelState.AddModelError(
                    "",
                    "Revisá los productos y las cantidades.");

            if (!ModelState.IsValid)
            {
                await CargarProductos();
                return View(modelo);
            }

            var fechaLocal = DateTime.UtcNow.AddHours(-6);

            if (await PeriodoCerradoAsync(fechaLocal))
            {
                ModelState.AddModelError(
                    "",
                    "El mes está cerrado. No se pueden registrar ventas en este período.");

                await CargarProductos();
                return View(modelo);
            }

            var cantidades = modelo.Detalles
                .GroupBy(d => d.ProductoId)
                .ToDictionary(g => g.Key, g => g.Sum(d => d.Cantidad));

            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var productos = await _context.Productos
                    .Where(p => cantidades.Keys.Contains(p.Id) && p.Activo)
                    .ToDictionaryAsync(p => p.Id);

                if (productos.Count != cantidades.Count)
                {
                    ModelState.AddModelError(
                        "",
                        "Uno o más productos no existen o están inactivos.");

                    await transaccion.RollbackAsync();
                    await CargarProductos();
                    return View(modelo);
                }

                foreach (var cantidad in cantidades)
                {
                    var producto = productos[cantidad.Key];

                    if (producto.Stock < cantidad.Value)
                    {
                        ModelState.AddModelError(
                            "",
                            $"No hay suficientes unidades de {producto.Nombre}. " +
                            $"Disponibles: {producto.Stock}. " +
                            $"Solicitadas: {cantidad.Value}.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    await transaccion.RollbackAsync();
                    await CargarProductos();
                    return View(modelo);
                }

                var nombreCuenta = modelo.MetodoPago;

                var cuenta = await _context.CuentasFinancieras
                    .FirstOrDefaultAsync(c => c.Nombre == nombreCuenta);

                if (cuenta == null)
                {
                    cuenta = new CuentaFinanciera
                    {
                        Nombre = nombreCuenta,
                        Activa = true,
                        FechaRegistro = DateTime.UtcNow
                    };

                    _context.CuentasFinancieras.Add(cuenta);
                    await _context.SaveChangesAsync();
                }

                var venta = new Venta
                {
                    Cliente = string.IsNullOrWhiteSpace(modelo.Cliente)
                        ? null : modelo.Cliente.Trim(),
                    MetodoPago = modelo.MetodoPago,
                    FechaRegistro = DateTime.UtcNow,
                    Usuario = User.Identity?.Name
                };

                foreach (var cantidad in cantidades)
                {
                    var producto = productos[cantidad.Key];
                    var unidades = cantidad.Value;

                    var subtotal = producto.PrecioVenta * unidades;
                    var ganancia =
                        (producto.PrecioVenta - producto.PrecioCompra) * unidades;

                    venta.Detalles.Add(new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = unidades,
                        PrecioUnitario = producto.PrecioVenta,
                        PrecioCompraUnitario = producto.PrecioCompra,
                        Subtotal = subtotal,
                        Ganancia = ganancia
                    });

                    producto.Stock -= unidades;
                }

                venta.Total = venta.Detalles.Sum(d => d.Subtotal);
                venta.Ganancia = venta.Detalles.Sum(d => d.Ganancia);

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                // INGRESO FINANCIERO AUTOMÁTICO DE LA VENTA
                _context.MovimientosFinancieros.Add(
                    new MovimientoFinanciero
                    {
                        CuentaFinancieraId = cuenta.Id,
                        VentaId = venta.Id,
                        Tipo = "Ingreso",
                        Categoria = "Venta",
                        Concepto = $"Venta #{venta.Id}",
                        Descripcion = $"Venta registrada mediante {venta.MetodoPago}",
                        Monto = venta.Total,
                        FechaRegistro = venta.FechaRegistro,
                        RegistradoPor = User.Identity?.Name,
                        AutorizadoPor = User.Identity?.Name,
                        EsGastoOperativo = false,
                        EsCompraInventario = false
                    });

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Mensaje"] =
                    "Venta registrada y movimiento financiero generado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();
                ModelState.AddModelError(
                    "",
                    "No se pudo registrar la venta. No se guardaron los cambios.");

                await CargarProductos();
                return View(modelo);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await using var transaccion =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var venta = await _context.Ventas
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (venta == null)
                {
                    await transaccion.RollbackAsync();
                    TempData["Error"] = "No se encontró la venta.";
                    return RedirectToAction(nameof(Index));
                }

                var fechaLocal = venta.FechaRegistro.AddHours(-6);

                if (await PeriodoCerradoAsync(fechaLocal))
                {
                    await transaccion.RollbackAsync();
                    TempData["Error"] =
                        "No podés eliminar una venta de un mes cerrado. Reabrí el período primero.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var detalle in venta.Detalles)
                    detalle.Producto.Stock += detalle.Cantidad;

                var movimiento = await _context.MovimientosFinancieros
                    .FirstOrDefaultAsync(m => m.VentaId == venta.Id);

                if (movimiento != null)
                    _context.MovimientosFinancieros.Remove(movimiento);

                _context.Ventas.Remove(venta);

                await _context.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Mensaje"] =
                    "Venta eliminada, ingreso revertido y productos devueltos al inventario.";
            }
            catch (Exception)
            {
                await transaccion.RollbackAsync();
                TempData["Error"] =
                    "No se pudo eliminar la venta. No se realizaron cambios.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> PeriodoCerradoAsync(DateTime fechaLocal)
        {
            var cierre = await _context.CierresMensuales
                .Where(c => c.Anio == fechaLocal.Year &&
                            c.Mes == fechaLocal.Month)
                .OrderByDescending(c => c.Version)
                .FirstOrDefaultAsync();

            return cierre?.Estado == "Cerrado";
        }

        private async Task CargarProductos()
        {
            ViewBag.Productos = await _context.Productos
                .Where(p => p.Activo && p.Stock > 0)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.MetodosPago = MetodosPago;
        }
    }
}
