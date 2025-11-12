using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.ViewModels.Products;

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

        // ===============================
        // INDEX (READ + SEARCH + FILTER)
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ProductFilterViewModel filter)
        {
            IQueryable<Product> query = _db.Products.AsNoTracking();

            // Search text (Name, SKU, Description)
            if (!string.IsNullOrWhiteSpace(filter.Q))
            {
                var q = filter.Q.Trim();
                query = query.Where(p =>
                    p.Name.Contains(q) ||
                    (p.Sku != null && p.Sku.Contains(q)) ||
                    (p.Description != null && p.Description.Contains(q)));
            }

            // Price filters
            if (filter.MinPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= filter.MaxPrice.Value);

            // Only active
            if (filter.OnlyActive == true)
                query = query.Where(p => p.IsActive);

            // Build list
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

        // ===============================
        // CREATE
        // ===============================
        [HttpGet]
        public IActionResult Create()
        {
            var vm = new ProductFormViewModel();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            // Validate SKU uniqueness
            if (!string.IsNullOrWhiteSpace(vm.Sku))
            {
                var sku = vm.Sku.Trim();
                bool exists = await _db.Products
                    .AsNoTracking()
                    .AnyAsync(p => p.Sku == sku);

                if (exists)
                {
                    ModelState.AddModelError(nameof(vm.Sku), "SKU must be unique.");
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
                Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
            };

            try
            {
                _db.Products.Add(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Products_Sku") == true)
                {
                    ModelState.AddModelError(nameof(vm.Sku), "SKU must be unique.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the product.");
                }

                return View(vm);
            }
        }

        // ===============================
        // EDIT
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var vm = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                UnitPrice = product.UnitPrice,
                Stock = product.Stock,
                IsActive = product.IsActive,
                Description = product.Description
            };

            return View(vm);
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

            // Unique SKU check
            if (!string.IsNullOrWhiteSpace(vm.Sku))
            {
                var sku = vm.Sku.Trim();
                bool exists = await _db.Products
                    .AsNoTracking()
                    .AnyAsync(p => p.Sku == sku && p.Id != vm.Id);

                if (exists)
                {
                    ModelState.AddModelError(nameof(vm.Sku), "SKU must be unique.");
                    return View(vm);
                }
            }

            // Update entity
            product.Name = vm.Name.Trim();
            product.Sku = string.IsNullOrWhiteSpace(vm.Sku) ? null : vm.Sku.Trim();
            product.UnitPrice = vm.UnitPrice;
            product.Stock = vm.Stock;
            product.IsActive = vm.IsActive;
            product.Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim();

            try
            {
                _db.Update(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Products_Sku") == true)
                {
                    ModelState.AddModelError(nameof(vm.Sku), "SKU must be unique.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the product.");
                }

                return View(vm);
            }
        }

        // ===============================
        // DELETE
        // ===============================
        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
                return NotFound();

            var vm = new ProductListItemViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                UnitPrice = product.UnitPrice,
                Stock = product.Stock,
                IsActive = product.IsActive
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            try
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the product.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
