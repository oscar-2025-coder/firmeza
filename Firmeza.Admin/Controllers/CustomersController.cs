using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: /Customers
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerListItemViewModel
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    DocumentNumber = c.DocumentNumber,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber
                })
                .ToListAsync();

            return View(customers);
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
                return View(model);
            }

            try
            {
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

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer created successfully.";

            return RedirectToAction(nameof(Create));
        }

        // GET: /Customers/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            var model = new CustomerEditViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                DocumentNumber = customer.DocumentNumber,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Age = customer.Age,
                IsActive = customer.IsActive
            };

            return View(model);
        }

        // POST: /Customers/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customer = await _context.Customers.FindAsync(model.Id);

            if (customer == null)
            {
                return NotFound();
            }

            customer.FullName = model.FullName!;
            customer.DocumentNumber = model.DocumentNumber!;
            customer.Email = model.Email!;
            customer.PhoneNumber = model.PhoneNumber!;
            customer.Age = model.Age;
            customer.IsActive = model.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Customers/Delete/{id}
        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound();
            }

            var model = new CustomerDeleteViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName
            };

            return View(model);
        }

        // POST: /Customers/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(CustomerDeleteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var customer = await _context.Customers.FindAsync(model.Id);

            if (customer == null)
            {
                return NotFound();
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
