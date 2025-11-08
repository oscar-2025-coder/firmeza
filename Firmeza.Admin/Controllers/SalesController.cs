using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Admin.Controllers
{
    [Authorize(Roles = "Administrator,Administrador")]
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}