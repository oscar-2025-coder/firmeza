using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.ViewModels.Products;
using OfficeOpenXml;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Admin.Controllers
{
    [Authorize(Roles = "Administrator,Administrador")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ============================================================
        // INDEX
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ProductFilterViewModel filter)
        {
            IQueryable<Product> query = _db.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Q))
            {
                var q = filter.Q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    (p.Sku != null && p.Sku.Contains(q)) ||
                    (p.Description != null && p.Description.Contains(q)));
            }

            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= filter.MaxPrice.Value);

            if (filter.OnlyActive == true)
                query = query.Where(p => p.IsActive);

            var products = await query
                .OrderBy(p => p.Name)
                .Select(p => new ProductListItemViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Sku = p.Sku,
                    UnitPrice = p.UnitPrice,
                    Stock = p.Stock,
                    IsActive = p.IsActive
                })
                .ToListAsync();

            ViewData["Filter"] = filter;
            return View(products);
        }

        // ============================================================
        // CREATE (GET)
        // ============================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View(new ProductFormViewModel());
        }

        // ============================================================
        // CREATE (POST)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Duplicate SKU check
            if (!string.IsNullOrWhiteSpace(vm.Sku))
            {
                var sku = vm.Sku.Trim();

                var existing = await _db.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Sku == sku);

                if (existing != null)
                {
                    vm.ExistingProductId = existing.Id;
                    vm.DuplicateMessage = $"Product with SKU '{sku}' already exists: {existing.Name}.";
                    return View(vm);
                }
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = vm.Name.Trim(),
                Sku = string.IsNullOrWhiteSpace(vm.Sku) ? null : vm.Sku.Trim(),
                UnitPrice = vm.UnitPrice,
                Stock = vm.Stock,
                IsActive = vm.IsActive,
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description?.Trim()
            };

            try
            {
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Database error while creating product.");
                return View(vm);
            }
        }

        // ============================================================
        // EDIT
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return View(new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                UnitPrice = product.UnitPrice,
                Stock = product.Stock,
                IsActive = product.IsActive,
                Description = product.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var product = await _db.Products.FindAsync(vm.Id);
            if (product == null)
                return NotFound();

            product.Name = vm.Name.Trim();
            product.Sku = string.IsNullOrWhiteSpace(vm.Sku) ? null : vm.Sku.Trim();
            product.UnitPrice = vm.UnitPrice;
            product.Stock = vm.Stock;
            product.IsActive = vm.IsActive;
            product.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description?.Trim();

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // DELETE (SOFT DELETE CORREGIDO)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var p = await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p == null)
                return NotFound();

            return View(new ProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                UnitPrice = p.UnitPrice,
                Stock = p.Stock,
                IsActive = p.IsActive
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            // 🔥 Check if product is used in sales (foreign key protection)
            bool hasSales = await _db.SaleItems.AnyAsync(s => s.ProductId == id);

            if (hasSales)
            {
                // Soft Delete → Just deactivate
                product.IsActive = false;
                await _db.SaveChangesAsync();

                TempData["ErrorMessage"] =
                    "⚠️ This product cannot be deleted because it has sales associated. It was deactivated instead.";

                return RedirectToAction(nameof(Index));
            }

            // Safe to delete
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Product deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // EXPORT TO EXCEL
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var products = await _db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .ToListAsync();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Products");

            sheet.Cells[1, 1].Value = "Name";
            sheet.Cells[1, 2].Value = "SKU";
            sheet.Cells[1, 3].Value = "Unit Price";
            sheet.Cells[1, 4].Value = "Stock";
            sheet.Cells[1, 5].Value = "Active";

            int row = 2;
            foreach (var p in products)
            {
                sheet.Cells[row, 1].Value = p.Name;
                sheet.Cells[row, 2].Value = p.Sku;
                sheet.Cells[row, 3].Value = p.UnitPrice;
                sheet.Cells[row, 4].Value = p.Stock;
                sheet.Cells[row, 5].Value = p.IsActive ? "Yes" : "No";
                row++;
            }

            var bytes = package.GetAsByteArray();
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "products.xlsx");
        }

        // ============================================================
        // EXPORT TO PDF
        // ============================================================
        public class ProductReportRow
        {
            public string Name { get; set; } = "";
            public string? Sku { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public bool Active { get; set; }
        }

        public class ProductPdfDocument : IDocument
        {
            public List<ProductReportRow> Items { get; }

            public ProductPdfDocument(List<ProductReportRow> items)
            {
                Items = items;
            }

            public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

            public DocumentSettings GetSettings() => DocumentSettings.Default;

            public void Compose(IDocumentContainer container)
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Size(PageSizes.A4);

                    page.Header().Text("Products Report")
                        .FontSize(20)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1);
                        });

                        table.Header(h =>
                        {
                            h.Cell().BorderBottom(1).Text("Name").SemiBold();
                            h.Cell().BorderBottom(1).Text("SKU").SemiBold();
                            h.Cell().BorderBottom(1).Text("Price").SemiBold();
                            h.Cell().BorderBottom(1).Text("Stock").SemiBold();
                            h.Cell().BorderBottom(1).Text("Active").SemiBold();
                        });

                        foreach (var p in Items)
                        {
                            table.Cell().BorderBottom(0.5f).Text(p.Name);
                            table.Cell().BorderBottom(0.5f).Text(p.Sku ?? "");
                            table.Cell().BorderBottom(0.5f).Text(p.Price.ToString("0.00"));
                            table.Cell().BorderBottom(0.5f).Text(p.Stock.ToString());
                            table.Cell().BorderBottom(0.5f).Text(p.Active ? "Yes" : "No");
                        }
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportToPdf()
        {
            var items = await _db.Products
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ProductReportRow
                {
                    Name = p.Name,
                    Sku = p.Sku,
                    Price = p.UnitPrice,
                    Stock = p.Stock,
                    Active = p.IsActive
                })
                .ToListAsync();

            var doc = new ProductPdfDocument(items);
            var pdf = doc.GeneratePdf();

            return File(pdf, "application/pdf", $"products-report-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf");
        }
    }
}
