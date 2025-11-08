using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Firmeza.Admin.Models;

namespace Firmeza.Admin.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index() => View();

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