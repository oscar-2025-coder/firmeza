using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.Services.Pdf;
using Firmeza.Admin.ViewModels.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Admin.Controllers
{
    public class SalesReportItem
    {
        public DateTimeOffset Date { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int ItemsCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SalesReportDocument : IDocument
    {
        public IList<SalesReportItem> Sales { get; }

        public SalesReportDocument(IList<SalesReportItem> sales)
        {
            Sales = sales;
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
                    .Text("Sales Report")
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
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(3); // Customer
                            columns.RelativeColumn(1); // Items
                            columns.RelativeColumn(2); // Subtotal
                            columns.RelativeColumn(2); // Tax
                            columns.RelativeColumn(2); // Total
                            columns.RelativeColumn(2); // Status
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("Date");
                            header.Cell().Element(HeaderCellStyle).Text("Customer");
                            header.Cell().Element(HeaderCellStyle).Text("Items");
                            header.Cell().Element(HeaderCellStyle).Text("Subtotal");
                            header.Cell().Element(HeaderCellStyle).Text("Tax");
                            header.Cell().Element(HeaderCellStyle).Text("Total");
                            header.Cell().Element(HeaderCellStyle).Text("Status");
                        });

                        foreach (var sale in Sales)
                        {
                            table.Cell().Element(CellStyle).Text(sale.Date.ToString("yyyy-MM-dd HH:mm"));
                            table.Cell().Element(CellStyle).Text(sale.CustomerName);
                            table.Cell().Element(CellStyle).Text(sale.ItemsCount.ToString());
                            table.Cell().Element(CellStyle).Text(sale.Subtotal.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(sale.Tax.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(sale.Total.ToString("0.00"));
                            table.Cell().Element(CellStyle).Text(sale.Status);
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
    public class SalesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ReceiptPdfService _receiptPdfService;
        private readonly IWebHostEnvironment _environment;

        public SalesController(
            ApplicationDbContext context,
            ReceiptPdfService receiptPdfService,
            IWebHostEnvironment environment)
        {
            _context = context;
            _receiptPdfService = receiptPdfService;
            _environment = environment;
        }

        // GET: /Sales
        public async Task<IActionResult> Index()
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return View(sales);
        }

        // GET: /Sales/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new SaleCreateViewModel();

            // Load active customers
            viewModel.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FullName} - {c.DocumentNumber}"
                })
                .ToListAsync();

            // Load active products
            viewModel.Products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} (${p.UnitPrice})"
                })
                .ToListAsync();

            // Start with one empty item
            viewModel.Items.Add(new SaleItemInputViewModel());

            return View(viewModel);
        }

        // POST: /Sales/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateViewModel model)
        {
            // Reload dropdown lists (needed if we re-display the view with validation errors)
            model.Customers = await _context.Customers
                .Where(c => c.IsActive)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FullName} - {c.DocumentNumber}"
                })
                .ToListAsync();

            model.Products = await _context.Products
                .Where(p => p.IsActive)
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = $"{p.Name} (${p.UnitPrice})"
                })
                .ToListAsync();

            // Must have at least one item
            if (model.Items == null || !model.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "The sale must contain at least one item.");
            }

            // Calculate totals from items
            const decimal taxRate = 0.19m; // 19% IVA example
            decimal subtotal = 0;

            if (model.Items != null)
            {
                foreach (var item in model.Items)
                {
                    if (item.Quantity > 0 && item.UnitPrice > 0)
                    {
                        subtotal += item.Quantity * item.UnitPrice;
                    }
                }
            }

            model.Subtotal = subtotal;
            model.Tax = Math.Round(subtotal * taxRate, 2);
            model.Total = model.Subtotal + model.Tax;

            if (!ModelState.IsValid)
            {
                // Show form again with validation errors
                return View(model);
            }

            // Map ViewModel to entity Sale
            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                CustomerId = model.CustomerId,
                Date = DateTimeOffset.UtcNow,
                Subtotal = model.Subtotal,
                Tax = model.Tax,
                Total = model.Total,
                Status = SaleStatus.Confirmed,
                Notes = model.Notes
            };

            // Create SaleItems
            foreach (var itemVm in model.Items)
            {
                var saleItem = new SaleItem
                {
                    Id = Guid.NewGuid(),
                    SaleId = sale.Id,
                    ProductId = itemVm.ProductId,
                    Quantity = itemVm.Quantity,
                    UnitPrice = itemVm.UnitPrice,
                    Amount = itemVm.Quantity * itemVm.UnitPrice
                };

                sale.Items.Add(saleItem);
            }

            // Save sale and items
            _context.Sales.Add(sale);
            await _context.SaveChangesAsync();

            // Reload sale with navigation properties for the PDF
            var savedSale = await _context.Sales
                .Include(s => s.Customer)
                .Include(s => s.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.Id == sale.Id);

            if (savedSale != null)
            {
                // Generate PDF in memory
                var pdfBytes = _receiptPdfService.GenerateReceiptPdf(savedSale);

                // Ensure receipts folder exists
                var receiptsFolder = Path.Combine(_environment.WebRootPath, "receipts");
                if (!Directory.Exists(receiptsFolder))
                {
                    Directory.CreateDirectory(receiptsFolder);
                }

                // File name based on sale Id
                var fileName = $"receipt_{savedSale.Id}.pdf";
                var filePath = Path.Combine(receiptsFolder, fileName);

                // Save PDF file to disk
                await System.IO.File.WriteAllBytesAsync(filePath, pdfBytes);

                // Store file name in the sale
                savedSale.ReceiptFileName = fileName;
                _context.Sales.Update(savedSale);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Sales/DownloadReceipt/{id}
        [HttpGet]
        public async Task<IActionResult> DownloadReceipt(Guid id)
        {
            // 1) Buscar la venta
            var sale = await _context.Sales
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sale == null)
            {
                return NotFound("Sale not found");
            }

            if (string.IsNullOrEmpty(sale.ReceiptFileName))
            {
                return NotFound("Sale has no receipt file");
            }

            // 2) Ruta física del archivo
            var receiptsFolder = Path.Combine(_environment.WebRootPath, "receipts");
            var filePath = Path.Combine(receiptsFolder, sale.ReceiptFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound("Receipt file not found on disk");
            }

            // 3) Devolver el PDF
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            const string contentType = "application/pdf";
            var downloadName = sale.ReceiptFileName;

            return File(fileBytes, contentType, downloadName);
        }

        // ===============================
        // EXPORT SALES TO EXCEL
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var sales = await _context.Sales
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            // EPPlus license
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Sales");

            // Headers
            worksheet.Cells[1, 1].Value = "Date";
            worksheet.Cells[1, 2].Value = "Customer";
            worksheet.Cells[1, 3].Value = "Items Count";
            worksheet.Cells[1, 4].Value = "Subtotal";
            worksheet.Cells[1, 5].Value = "Tax";
            worksheet.Cells[1, 6].Value = "Total";
            worksheet.Cells[1, 7].Value = "Status";

            var row = 2;
            foreach (var s in sales)
            {
                worksheet.Cells[row, 1].Value = s.Date.ToString("yyyy-MM-dd HH:mm");
                worksheet.Cells[row, 2].Value = s.Customer.FullName;
                worksheet.Cells[row, 3].Value = s.Items.Count;
                worksheet.Cells[row, 4].Value = s.Subtotal;
                worksheet.Cells[row, 5].Value = s.Tax;
                worksheet.Cells[row, 6].Value = s.Total;
                worksheet.Cells[row, 7].Value = s.Status.ToString();
                row++;
            }

            var bytes = package.GetAsByteArray();

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "sales.xlsx"
            );
        }

        // ===============================
        // EXPORT SALES TO PDF
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var sales = await _context.Sales
                .AsNoTracking()
                .Include(s => s.Customer)
                .Include(s => s.Items)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            var items = sales
                .Select(s => new SalesReportItem
                {
                    Date = s.Date,
                    CustomerName = s.Customer.FullName,
                    ItemsCount = s.Items.Count,
                    Subtotal = s.Subtotal,
                    Tax = s.Tax,
                    Total = s.Total,
                    Status = s.Status.ToString()
                })
                .ToList();

            var document = new SalesReportDocument(items);

            var pdfBytes = document.GeneratePdf();
            var fileName = $"sales-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
    }
}
