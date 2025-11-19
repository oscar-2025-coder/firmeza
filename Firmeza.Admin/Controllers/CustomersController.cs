using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.ViewModels.Customers;
using OfficeOpenXml; // EPPlus para Excel
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Admin.Controllers
{
    public class CustomerReportItem
    {
        public string FullName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
    }

    public class CustomersReportDocument : IDocument
    {
        public IList<CustomerReportItem> Customers { get; }

        public CustomersReportDocument(IList<CustomerReportItem> customers)
        {
            Customers = customers;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);

                page.Header()
                    .Text("Customers Report")
                    .SemiBold()
                    .FontSize(20)
                    .AlignCenter();

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm}");

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Full Name
                            columns.RelativeColumn(2); // Document
                            columns.RelativeColumn(3); // Email
                            columns.RelativeColumn(2); // Phone
                            columns.RelativeColumn(1); // Age
                            columns.RelativeColumn(1); // Active
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Full Name");
                            header.Cell().Element(HeaderCellStyle).Text("Document");
                            header.Cell().Element(HeaderCellStyle).Text("Email");
                            header.Cell().Element(HeaderCellStyle).Text("Phone");
                            header.Cell().Element(HeaderCellStyle).Text("Age");
                            header.Cell().Element(HeaderCellStyle).Text("Active");
                        });

                        foreach (var customer in Customers)
                        {
                            table.Cell().Element(CellStyle).Text(customer.FullName);
                            table.Cell().Element(CellStyle).Text(customer.DocumentNumber);
                            table.Cell().Element(CellStyle).Text(customer.Email ?? string.Empty);
                            table.Cell().Element(CellStyle).Text(customer.PhoneNumber ?? string.Empty);
                            table.Cell().Element(CellStyle).Text(customer.Age.ToString());
                            table.Cell().Element(CellStyle).Text(customer.IsActive ? "Yes" : "No");
                        }

                        static IContainer HeaderCellStyle(IContainer container)
                        {
                            return container
                                .PaddingVertical(5)
                                .DefaultTextStyle(x => x.SemiBold())
                                .BorderBottom(1);
                        }

                        static IContainer CellStyle(IContainer container)
                        {
                            return container
                                .PaddingVertical(5)
                                .BorderBottom(0.5f);
                        }
                    });
                });

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
            });
        }
    }

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

        // ===============================
        // EXPORT TO EXCEL
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.FullName)
                .ToListAsync();

            // EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Customers");

            // Headers
            worksheet.Cells[1, 1].Value = "Full Name";
            worksheet.Cells[1, 2].Value = "Document";
            worksheet.Cells[1, 3].Value = "Email";
            worksheet.Cells[1, 4].Value = "Phone";
            worksheet.Cells[1, 5].Value = "Age";
            worksheet.Cells[1, 6].Value = "Active";

            // Data
            var row = 2;
            foreach (var c in customers)
            {
                worksheet.Cells[row, 1].Value = c.FullName;
                worksheet.Cells[row, 2].Value = c.DocumentNumber;
                worksheet.Cells[row, 3].Value = c.Email;
                worksheet.Cells[row, 4].Value = c.PhoneNumber;
                worksheet.Cells[row, 5].Value = c.Age;
                worksheet.Cells[row, 6].Value = c.IsActive ? "Yes" : "No";
                row++;
            }

            var bytes = package.GetAsByteArray();

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "customers.xlsx"
            );
        }

        // ===============================
        // EXPORT TO PDF
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.FullName)
                .ToListAsync();

            var items = customers
                .Select(c => new CustomerReportItem
                {
                    FullName = c.FullName,
                    DocumentNumber = c.DocumentNumber,
                    Email = c.Email,
                    PhoneNumber = c.PhoneNumber,
                    Age = c.Age,
                    IsActive = c.IsActive
                })
                .ToList();

            var document = new CustomersReportDocument(items);

            var pdfBytes = document.GeneratePdf();
            var fileName = $"customers-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
