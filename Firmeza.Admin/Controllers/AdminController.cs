using Firmeza.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Admin.Controllers
{
    [Authorize(Roles = "Administrator,Administrador")]
    public class AdminController : Controller
    {
        // GET: /Admin/Index
        public IActionResult Index()
        {
            var viewModel = new DashboardMetricsViewModel
            {
                TotalProducts = 0,
                TotalCustomers = 0,
                TotalSales = 0,
                RevenueLast30Days = 0m
            };

            return View(viewModel);
        }
    }
}