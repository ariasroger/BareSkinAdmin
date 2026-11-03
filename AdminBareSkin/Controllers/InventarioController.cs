
using AdminBareSkin.Data;
using AdminBareSkin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AdminBareSkin.Controllers
{
    [Authorize(Roles = "SuperAdmin,Dueño")]
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LISTADO PRINCIPAL
        public async Task<IActionResult> Index(string? buscar)
        {
            var consulta = _context.Productos
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(p =>
                    p.Nombre.Contains(buscar));
            }

            var productos = await consulta
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Buscar = buscar;
            ViewBag.TotalProductos = productos.Count;
            ViewBag.TotalUnidades = productos.Sum(p => p.Stock);
            ViewBag.ValorInventario = productos.Sum(
                p => p.PrecioCompra * p.Stock);
            ViewBag.ValorVenta = productos.Sum(
                p => p.PrecioVenta * p.Stock);
            ViewBag.GananciaPotencial = productos.Sum(
                p => p.GananciaPorUnidad * p.Stock);

            return View(productos);
        }

        // REGISTRAR PRODUCTO
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto)
        {
            if (!ModelState.IsValid)
                return View(producto);

            producto.Id = 0;
            producto.FechaRegistro = DateTime.UtcNow;
            producto.Activo = true;

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // EDITAR PRODUCTO
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto)
        {
            if (id != producto.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(producto);

            var existente = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (existente == null)
                return NotFound();

            existente.Nombre = producto.Nombre;
            existente.PrecioCompra = producto.PrecioCompra;
            existente.PrecioVenta = producto.PrecioVenta;
            existente.Stock = producto.Stock;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR PRODUCTO (BAJA LÓGICA)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if (producto == null)
                return NotFound();

            producto.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Producto eliminado del inventario.";

            return RedirectToAction(nameof(Index));
        }
    }
}
