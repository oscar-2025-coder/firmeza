using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.ViewModels.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace Firmeza.Admin.Controllers;

[Authorize(Roles = "Administrator,Administrador")]
public class DataImportController : Controller
{
    private readonly ApplicationDbContext _context;

    public DataImportController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult BulkImport()
    {
        var viewModel = new BulkImportViewModel();
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BulkImport(BulkImportViewModel model)
    {
        if (!ModelState.IsValid || model.File == null || model.File.Length == 0)
        {
            ModelState.AddModelError("File", "Please select a valid Excel file.");
            return View(model);
        }

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                model.File.CopyTo(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null)
                    {
                        TempData["ErrorMessage"] = "The Excel file has no worksheets.";
                        return View(model);
                    }

                    int totalColumns = worksheet.Dimension.End.Column;
                    int totalRows = worksheet.Dimension.End.Row;

                    // ============ READ HEADERS (ROW 1) ============
                    var headers = new List<string>();
                    var columnIndexByName = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                    for (int col = 1; col <= totalColumns; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Text?.Trim() ?? string.Empty;
                        headers.Add(headerValue);

                        if (!string.IsNullOrWhiteSpace(headerValue) &&
                            !columnIndexByName.ContainsKey(headerValue))
                        {
                            columnIndexByName.Add(headerValue, col);
                        }
                    }

                    int? GetColumn(string name)
                    {
                        return columnIndexByName.TryGetValue(name, out var index)
                            ? index
                            : (int?)null;
                    }

                    // ============ READ DATA ROWS (FROM ROW 2) ============
                    var rawRows = new List<RawImportRowViewModel>();

                    for (int row = 2; row <= totalRows; row++)
                    {
                        bool isEmptyRow = true;
                        for (int col = 1; col <= totalColumns; col++)
                        {
                            if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                            {
                                isEmptyRow = false;
                                break;
                            }
                        }

                        if (isEmptyRow)
                        {
                            continue;
                        }

                        var rawRow = new RawImportRowViewModel
                        {
                            RowNumber = row
                        };

                        var productNameCol = GetColumn("ProductName");
                        if (productNameCol.HasValue)
                        {
                            rawRow.ProductName = worksheet.Cells[row, productNameCol.Value].Text?.Trim();
                        }

                        var priceCol = GetColumn("Price");
                        if (priceCol.HasValue)
                        {
                            rawRow.PriceText = worksheet.Cells[row, priceCol.Value].Text?.Trim();
                        }

                        var customerNameCol = GetColumn("CustomerName");
                        if (customerNameCol.HasValue)
                        {
                            rawRow.CustomerName = worksheet.Cells[row, customerNameCol.Value].Text?.Trim();
                        }

                        var emailCol = GetColumn("Email");
                        if (emailCol.HasValue)
                        {
                            rawRow.Email = worksheet.Cells[row, emailCol.Value].Text?.Trim();
                        }

                        var saleDateCol = GetColumn("SaleDate");
                        if (saleDateCol.HasValue)
                        {
                            rawRow.SaleDateText = worksheet.Cells[row, saleDateCol.Value].Text?.Trim();
                        }

                        var quantityCol = GetColumn("Quantity");
                        if (quantityCol.HasValue)
                        {
                            rawRow.QuantityText = worksheet.Cells[row, quantityCol.Value].Text?.Trim();
                        }

                        rawRows.Add(rawRow);
                    }

                    // ============ VALIDATION & ERROR LOG ============
                    var errors = new List<ImportErrorViewModel>();
                    var emailValidator = new EmailAddressAttribute();

                    var productsDict = new Dictionary<string, ImportedProductViewModel>(StringComparer.OrdinalIgnoreCase);
                    var customersDict = new Dictionary<string, ImportedCustomerViewModel>(StringComparer.OrdinalIgnoreCase);
                    var salesList = new List<ImportedSaleViewModel>();

                    foreach (var raw in rawRows)
                    {
                        // ---- PRODUCTS ----
                        if (string.IsNullOrWhiteSpace(raw.ProductName))
                        {
                            errors.Add(new ImportErrorViewModel
                            {
                                RowNumber = raw.RowNumber,
                                Message = "ProductName is required."
                            });
                        }
                        else
                        {
                            if (!productsDict.TryGetValue(raw.ProductName, out var importedProduct))
                            {
                                importedProduct = new ImportedProductViewModel
                                {
                                    ProductName = raw.ProductName
                                };
                                productsDict.Add(raw.ProductName, importedProduct);
                            }

                            if (!string.IsNullOrWhiteSpace(raw.PriceText))
                            {
                                if (decimal.TryParse(raw.PriceText, out var price))
                                {
                                    importedProduct.Price = price;
                                }
                                else
                                {
                                    errors.Add(new ImportErrorViewModel
                                    {
                                        RowNumber = raw.RowNumber,
                                        Message = $"Invalid price value '{raw.PriceText}' for product '{raw.ProductName}'."
                                    });
                                }
                            }

                            importedProduct.SourceRows.Add(raw.RowNumber);
                        }

                        // ---- CUSTOMERS ----
                        if (string.IsNullOrWhiteSpace(raw.CustomerName))
                        {
                            errors.Add(new ImportErrorViewModel
                            {
                                RowNumber = raw.RowNumber,
                                Message = "CustomerName is required."
                            });
                        }
                        else
                        {
                            if (!customersDict.TryGetValue(raw.CustomerName, out var importedCustomer))
                            {
                                importedCustomer = new ImportedCustomerViewModel
                                {
                                    CustomerName = raw.CustomerName,
                                    Email = raw.Email
                                };
                                customersDict.Add(raw.CustomerName, importedCustomer);
                            }

                            if (!string.IsNullOrWhiteSpace(raw.Email) &&
                                !emailValidator.IsValid(raw.Email))
                            {
                                errors.Add(new ImportErrorViewModel
                                {
                                    RowNumber = raw.RowNumber,
                                    Message = $"Invalid email address '{raw.Email}' for customer '{raw.CustomerName}'."
                                });
                            }

                            importedCustomer.SourceRows.Add(raw.RowNumber);
                        }

                        // ---- SALES ----
                        if (!string.IsNullOrWhiteSpace(raw.ProductName) &&
                            !string.IsNullOrWhiteSpace(raw.CustomerName))
                        {
                            DateTime? saleDate = null;
                            if (!string.IsNullOrWhiteSpace(raw.SaleDateText))
                            {
                                if (DateTime.TryParse(raw.SaleDateText, out var parsedDate))
                                {
                                    saleDate = parsedDate;
                                }
                                else
                                {
                                    errors.Add(new ImportErrorViewModel
                                    {
                                        RowNumber = raw.RowNumber,
                                        Message = $"Invalid sale date '{raw.SaleDateText}'."
                                    });
                                }
                            }

                            int? quantity = null;
                            if (!string.IsNullOrWhiteSpace(raw.QuantityText))
                            {
                                if (int.TryParse(raw.QuantityText, out var parsedQty))
                                {
                                    quantity = parsedQty;
                                }
                                else
                                {
                                    errors.Add(new ImportErrorViewModel
                                    {
                                        RowNumber = raw.RowNumber,
                                        Message = $"Invalid quantity '{raw.QuantityText}'."
                                    });
                                }
                            }

                            var sale = new ImportedSaleViewModel
                            {
                                ProductName = raw.ProductName!,
                                CustomerName = raw.CustomerName!,
                                SaleDate = saleDate,
                                Quantity = quantity,
                                SourceRow = raw.RowNumber
                            };

                            salesList.Add(sale);
                        }
                    }

                    var products = productsDict.Values.ToList();
                    var customers = customersDict.Values.ToList();
                    var sales = salesList;
                    var errorCount = errors.Count;

                    // Llenar el ViewModel para la vista
                    model.TotalRows = rawRows.Count;
                    model.ProductsCount = products.Count;
                    model.CustomersCount = customers.Count;
                    model.SalesCount = sales.Count;
                    model.ErrorCount = errorCount;
                    model.Errors = errors;

                    // Si hay errores, no guardamos nada
                    if (errorCount > 0)
                    {
                        TempData["ErrorMessage"] =
                            $"Data loaded and validated, but there are {errorCount} error(s). Nothing was saved.";
                        return View(model);
                    }

                    // ============ PERSISTENCE (UPSERT) ============
                    using var transaction = _context.Database.BeginTransaction();

                    // ---- UPSERT PRODUCTS ----
                    var existingProducts = _context.Products.ToList();
                    var productEntitiesByName = existingProducts
                        .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

                    foreach (var importedProduct in products)
                    {
                        if (productEntitiesByName.TryGetValue(importedProduct.ProductName, out var existing))
                        {
                            // Update
                            if (importedProduct.Price.HasValue)
                            {
                                existing.UnitPrice = importedProduct.Price.Value;
                            }
                            existing.IsActive = true;
                        }
                        else
                        {
                            var newProduct = new Product
                            {
                                Id = Guid.NewGuid(),
                                Name = importedProduct.ProductName,
                                UnitPrice = importedProduct.Price ?? 0m,
                                IsActive = true,
                                Stock = 0
                            };
                            _context.Products.Add(newProduct);
                            productEntitiesByName.Add(newProduct.Name, newProduct);
                        }
                    }

                    // ---- UPSERT CUSTOMERS ----
                    var existingCustomers = _context.Customers.ToList();
                    var customerEntitiesByName = existingCustomers
                        .ToDictionary(c => c.FullName, StringComparer.OrdinalIgnoreCase);

                    foreach (var importedCustomer in customers)
                    {
                        if (customerEntitiesByName.TryGetValue(importedCustomer.CustomerName, out var existing))
                        {
                            // Update basic fields
                            if (!string.IsNullOrWhiteSpace(importedCustomer.Email))
                            {
                                existing.Email = importedCustomer.Email!;
                            }

                            existing.IsActive = true;
                        }
                        else
                        {
                            var newCustomer = new Customer
                            {
                                Id = Guid.NewGuid(),
                                FullName = importedCustomer.CustomerName,
                                Email = importedCustomer.Email ?? string.Empty,
                                DocumentNumber = string.Empty, // no viene en el Excel
                                PhoneNumber = string.Empty,     // no viene en el Excel
                                Age = 0,                        // no viene en el Excel
                                IsActive = true
                            };
                            _context.Customers.Add(newCustomer);
                            customerEntitiesByName.Add(newCustomer.FullName, newCustomer);
                        }
                    }

                    _context.SaveChanges();

                    // ---- INSERT SALES ----
                    var taxRate = 0.19m; // ejemplo de IVA 19%
                    foreach (var importedSale in sales)
                    {
                        if (!productEntitiesByName.TryGetValue(importedSale.ProductName, out var productEntity))
                        {
                            errors.Add(new ImportErrorViewModel
                            {
                                RowNumber = importedSale.SourceRow,
                                Message = $"Product '{importedSale.ProductName}' not found when creating sale."
                            });
                            continue;
                        }

                        if (!customerEntitiesByName.TryGetValue(importedSale.CustomerName, out var customerEntity))
                        {
                            errors.Add(new ImportErrorViewModel
                            {
                                RowNumber = importedSale.SourceRow,
                                Message = $"Customer '{importedSale.CustomerName}' not found when creating sale."
                            });
                            continue;
                        }

                        var quantity = importedSale.Quantity ?? 1;
                        var unitPrice = productEntity.UnitPrice;
                        var amount = unitPrice * quantity;
                        var subtotal = amount;
                        var tax = subtotal * taxRate;
                        var total = subtotal + tax;

                        var saleEntity = new Sale
                        {
                            Id = Guid.NewGuid(),
                            CustomerId = customerEntity.Id,
                            Date = importedSale.SaleDate.HasValue
                                ? new DateTimeOffset(importedSale.SaleDate.Value)
                                : DateTimeOffset.Now,
                            Subtotal = subtotal,
                            Tax = tax,
                            Total = total,
                            Status = SaleStatus.Confirmed,
                            Notes = "Imported from Excel"
                        };

                        var saleItem = new SaleItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = productEntity.Id,
                            Quantity = quantity,
                            UnitPrice = unitPrice,
                            Amount = amount
                        };

                        saleEntity.Items.Add(saleItem);
                        _context.Sales.Add(saleEntity);
                    }

                    _context.SaveChanges();
                    transaction.Commit();

                    model.ErrorCount = errors.Count;
                    model.Errors = errors;

                    TempData["SuccessMessage"] =
                        $"Import completed. Rows: {rawRows.Count}, " +
                        $"Products: {products.Count}, Customers: {customers.Count}, Sales: {sales.Count}. " +
                        (errors.Count > 0
                            ? $"Warnings: {errors.Count} issue(s) while creating sales."
                            : "All records saved successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"ERROR: {ex.Message}";
        }


        return View(model);
    }
}
