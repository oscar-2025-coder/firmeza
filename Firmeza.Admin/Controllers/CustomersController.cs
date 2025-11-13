using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.ViewModels.Customers;

namespace Firmeza.Admin.Controllers
{
    [Authorize(Roles = "Administrator,Administrador")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // GET: /Customers/Create
        public IActionResult Create()
        {
            var model = new CustomerCreateViewModel();
            return View(model);
        }

        // POST: /Customers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // DataAnnotations validations failed
                return View(model);
            }

            try
            {
                // Validate AgeText
                if (!string.IsNullOrWhiteSpace(model.AgeText))
                {
                    model.Age = int.Parse(model.AgeText);
                }
                else
                {
                    model.ErrorMessage = "Age is required and must be an integer.";
                    return View(model);
                }
            }
            catch (FormatException)
            {
                model.ErrorMessage = "Age must be a valid integer number.";
                return View(model);
            }
            catch (OverflowException)
            {
                model.ErrorMessage = "Age number is too large.";
                return View(model);
            }

            // Create entity
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = model.FullName!,
                DocumentNumber = model.DocumentNumber!,
                Email = model.Email!,
                PhoneNumber = model.PhoneNumber!,
                Age = model.Age!.Value,
                IsActive = true
            };

            // Save into database
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Customer created successfully.";

            return RedirectToAction(nameof(Create));
        }
    }
}
