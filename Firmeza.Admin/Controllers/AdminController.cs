using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.Admin.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}