using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Firmeza.Admin.Models; 

namespace Firmeza.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Creamos los datos iniciales en 0 para que el Dashboard cargue sin errores
            var model = new DashboardMetricsViewModel
            {
                TotalProducts = 0,
                TotalCustomers = 0,
                TotalSales = 0,
                RevenueLast30Days = 0
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var vm = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            };
            return View(vm);
        }
    }
}