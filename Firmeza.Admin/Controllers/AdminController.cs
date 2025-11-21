using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Admin.Controllers
{
    [Authorize(Roles = "Administrator,Administrador")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            // --- Conteos ---
            int totalProducts = await _db.Products.CountAsync();
            int totalCustomers = await _db.Customers.CountAsync();
            int totalSales = await _db.Sales.CountAsync();

            // --- Ventas últimos 30 días ---
            var last30days = await _db.Sales
                .Where(s => s.Date >= DateTimeOffset.UtcNow.AddDays(-30))
                .ToListAsync();

            // Calcular correctamente ingresos: Subtotal + Tax
            decimal revenueLast30Days = last30days.Sum(s => s.Subtotal + s.Tax);

            // Crear VM
            var viewModel = new DashboardMetricsViewModel
            {
                TotalProducts = totalProducts,
                TotalCustomers = totalCustomers,
                TotalSales = totalSales,
                RevenueLast30Days = revenueLast30Days
            };

            return View(viewModel);
        }
    }
}