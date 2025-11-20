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
            var viewModel = new DashboardMetricsViewModel
            {
                TotalProducts = await _db.Products.CountAsync(),
                TotalCustomers = await _db.Customers.CountAsync(),
                TotalSales = await _db.Sales.CountAsync(),

                // Revenue from last 30 days
                RevenueLast30Days = await _db.Sales
                    .Where(s => s.Date >= DateTimeOffset.UtcNow.AddDays(-30))
                    .SumAsync(s => (decimal?)s.Total) ?? 0m
            };

            return View(viewModel);
        }
    }
}