using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

        // -----------------------------------------------------
        // INDEX
        // -----------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.FullName)
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

        // -----------------------------------------------------
        // CREATE GET
        // -----------------------------------------------------
        public IActionResult Create()
        {
            return View(new CustomerCreateViewModel());
        }

        // -----------------------------------------------------
        // CREATE POST
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCreateViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (!int.TryParse(vm.AgeText, out var parsedAge))
            {
                vm.ErrorMessage = "Age must be a valid number.";
                return View(vm);
            }

            if (await _context.Customers.AnyAsync(c => c.DocumentNumber == vm.DocumentNumber))
            {
                vm.ErrorMessage = "Document number is already registered.";
                return View(vm);
            }

            if (await _context.Customers.AnyAsync(c => c.Email == vm.Email))
            {
                vm.ErrorMessage = "Email is already registered.";
                return View(vm);
            }

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = vm.FullName!,
                DocumentNumber = vm.DocumentNumber!,
                Email = vm.Email!,
                PhoneNumber = vm.PhoneNumber!,
                Age = parsedAge,
                IsActive = true
            };

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------
        // EDIT GET
        // -----------------------------------------------------
        public async Task<IActionResult> Edit(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return RedirectToAction(nameof(Index));

            return View(new CustomerEditViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                DocumentNumber = customer.DocumentNumber,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Age = customer.Age,
                IsActive = customer.IsActive
            });
        }

        // -----------------------------------------------------
        // EDIT POST
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CustomerEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var customer = await _context.Customers.FindAsync(vm.Id);
            if (customer == null)
                return RedirectToAction(nameof(Index));

            customer.FullName = vm.FullName!;
            customer.DocumentNumber = vm.DocumentNumber!;
            customer.Email = vm.Email!;
            customer.PhoneNumber = vm.PhoneNumber!;
            customer.Age = vm.Age;
            customer.IsActive = vm.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------
        // DELETE GET
        // -----------------------------------------------------
        public async Task<IActionResult> Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return RedirectToAction(nameof(Index));

            bool hasSales = await _context.Sales.AnyAsync(s => s.CustomerId == id);

            return View(new CustomerDeleteViewModel
            {
                Id = customer.Id,
                FullName = customer.FullName,
                HasRelatedSales = hasSales
            });
        }

        // -----------------------------------------------------
        // DELETE POST
        // -----------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return RedirectToAction(nameof(Index));

            bool hasSales = await _context.Sales.AnyAsync(s => s.CustomerId == id);
            if (hasSales)
            {
                TempData["ErrorMessage"] = "This customer has sales and cannot be deleted.";
                return RedirectToAction(nameof(Index));
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // -----------------------------------------------------
        // EXPORT EXCEL
        // -----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Customers");

            ws.Cells[1, 1].Value = "Full Name";
            ws.Cells[1, 2].Value = "Document";
            ws.Cells[1, 3].Value = "Email";
            ws.Cells[1, 4].Value = "Phone";
            ws.Cells[1, 5].Value = "Age";
            ws.Cells[1, 6].Value = "Active";

            int row = 2;
            foreach (var c in customers)
            {
                ws.Cells[row, 1].Value = c.FullName;
                ws.Cells[row, 2].Value = c.DocumentNumber;
                ws.Cells[row, 3].Value = c.Email;
                ws.Cells[row, 4].Value = c.PhoneNumber;
                ws.Cells[row, 5].Value = c.Age;
                ws.Cells[row, 6].Value = c.IsActive ? "Yes" : "No";
                row++;
            }

            return File(
                package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "customers.xlsx"
            );
        }

        // -----------------------------------------------------
        // EXPORT PDF
        // -----------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.FullName)
                .ToListAsync();

            var items = customers.Select(c => new CustomerReportItem
            {
                FullName = c.FullName,
                DocumentNumber = c.DocumentNumber,
                Email = c.Email,
                PhoneNumber = c.PhoneNumber,
                Age = c.Age,
                IsActive = c.IsActive
            }).ToList();

            var doc = new CustomersReportDocument(items);
            var pdfBytes = doc.GeneratePdf();

            return File(pdfBytes, "application/pdf",
                $"customers-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
    }

    // -----------------------------------------------------
    // PDF DTO
    // -----------------------------------------------------
    public class CustomerReportItem
    {
        public string FullName { get; set; } = "";
        public string DocumentNumber { get; set; } = "";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    // -----------------------------------------------------
    // PDF DOCUMENT
    // -----------------------------------------------------
    public class CustomersReportDocument : IDocument
    {
        private readonly IList<CustomerReportItem> _customers;

        public CustomersReportDocument(IList<CustomerReportItem> customers)
        {
            _customers = customers;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header()
                    .Text("Customers Report")
                    .FontSize(20)
                    .SemiBold()
                    .AlignCenter();

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(1);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell().Text("Full Name").SemiBold();
                        header.Cell().Text("Document").SemiBold();
                        header.Cell().Text("Email").SemiBold();
                        header.Cell().Text("Phone").SemiBold();
                        header.Cell().Text("Age").SemiBold();
                        header.Cell().Text("Active").SemiBold();
                    });

                    foreach (var c in _customers)
                    {
                        table.Cell().Text(c.FullName);
                        table.Cell().Text(c.DocumentNumber);
                        table.Cell().Text(c.Email ?? "");
                        table.Cell().Text(c.PhoneNumber ?? "");
                        table.Cell().Text(c.Age);
                        table.Cell().Text(c.IsActive ? "Yes" : "No");
                    }
                });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
            });
        }
    }
}
