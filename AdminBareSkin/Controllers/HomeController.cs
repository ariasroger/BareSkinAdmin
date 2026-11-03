
using AdminBareSkin.Data;
using AdminBareSkin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace AdminBareSkin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Panel principal";

            var productos = _context.Productos
                .Where(p => p.Activo);

            ViewBag.TotalProductos = await productos.CountAsync();

            ViewBag.TotalUnidades = await productos
                .SumAsync(p => (int?)p.Stock) ?? 0;

            ViewBag.ValorInventario = await productos
                .SumAsync(p => (decimal?)(p.PrecioCompra * p.Stock)) ?? 0m;

            ViewBag.ValorVenta = await productos
                .SumAsync(p => (decimal?)(p.PrecioVenta * p.Stock)) ?? 0m;

            return View();
        }

        [HttpGet]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public async Task<IActionResult> ValorInventario()
        {
            var productos = _context.Productos
                .Where(p => p.Activo);

            var valorInventario = await productos
                .SumAsync(p => (decimal?)(p.PrecioCompra * p.Stock)) ?? 0m;

            var valorVenta = await productos
                .SumAsync(p => (decimal?)(p.PrecioVenta * p.Stock)) ?? 0m;

            var totalProductos = await productos.CountAsync();

            var totalUnidades = await productos
                .SumAsync(p => (int?)p.Stock) ?? 0;

            return Json(new
            {
                valorInventario,
                valorVenta,
                totalProductos,
                totalUnidades
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
