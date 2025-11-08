using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Admin.Controllers
{
    [Authorize(Roles = "Administrator,Administrador")]
    public class ProductsController : Controller
    {
        // GET: /Products/Index
        public IActionResult Index()
        {
            return View();
        }
    }
}