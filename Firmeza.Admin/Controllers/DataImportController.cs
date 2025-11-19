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

    // GET: /DataImport/BulkImport
    [HttpGet]
    public IActionResult BulkImport()
    {
        return View(new BulkImportViewModel());
    }

    // POST: /DataImport/BulkImport
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult BulkImport(BulkImportViewModel model)
    {
        if (model.File == null || model.File.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a valid .xlsx file.";
            return View(model);
        }

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var stream = new MemoryStream();
            model.File.CopyTo(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                TempData["ErrorMessage"] = "Excel file is empty or corrupted.";
                return View(model);
            }

            int totalColumns = worksheet.Dimension.End.Column;
            int totalRows = worksheet.Dimension.End.Row;

            // ===========================
            // READ HEADERS
            // ===========================
            var columnIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int col = 1; col <= totalColumns; col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(header) && !columnIndex.ContainsKey(header))
                {
                    columnIndex.Add(header, col);
                }
            }

            int? GetColumn(string name) =>
                columnIndex.TryGetValue(name, out var index) ? index : (int?)null;

            // ===========================
            // READ RAW ROWS
            // ===========================
            var rawRows = new List<RawImportRowViewModel>();

            for (int row = 2; row <= totalRows; row++)
            {
                var isEmpty = true;
                for (int col = 1; col <= totalColumns; col++)
                {
                    if (!string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Text))
                    {
                        isEmpty = false;
                        break;
                    }
                }
                if (isEmpty) continue;

                var raw = new RawImportRowViewModel { RowNumber = row };

                if (GetColumn("ProductName") is int pcol)
                    raw.ProductName = worksheet.Cells[row, pcol].Text.Trim();

                if (GetColumn("Price") is int pricecol)
                    raw.PriceText = worksheet.Cells[row, pricecol].Text.Trim();

                if (GetColumn("CustomerName") is int ccol)
                    raw.CustomerName = worksheet.Cells[row, ccol].Text.Trim();

                if (GetColumn("Email") is int ecol)
                    raw.Email = worksheet.Cells[row, ecol].Text.Trim();

                if (GetColumn("SaleDate") is int dcol)
                    raw.SaleDateText = worksheet.Cells[row, dcol].Text.Trim();

                if (GetColumn("Quantity") is int qcol)
                    raw.QuantityText = worksheet.Cells[row, qcol].Text.Trim();

                rawRows.Add(raw);
            }

            // ===========================
            // VALIDATION + NORMALIZATION
            // ===========================
            var emailValidator = new EmailAddressAttribute();
            var errors = new List<ImportErrorViewModel>();

            var productsDict = new Dictionary<string, ImportedProductViewModel>(StringComparer.OrdinalIgnoreCase);
            var customersDict = new Dictionary<string, ImportedCustomerViewModel>(StringComparer.OrdinalIgnoreCase);
            var salesList = new List<ImportedSaleViewModel>();

            foreach (var raw in rawRows)
            {
                // — PRODUCTS —
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
                    if (!productsDict.TryGetValue(raw.ProductName, out var prod))
                    {
                        prod = new ImportedProductViewModel { ProductName = raw.ProductName };
                        productsDict.Add(raw.ProductName, prod);
                    }

                    if (!string.IsNullOrWhiteSpace(raw.PriceText))
                    {
                        if (decimal.TryParse(raw.PriceText, out var parsedPrice))
                        {
                            prod.Price = parsedPrice;
                        }
                        else
                        {
                            errors.Add(new ImportErrorViewModel
                            {
                                RowNumber = raw.RowNumber,
                                Message = $"Invalid price '{raw.PriceText}'."
                            });
                        }
                    }
                }

                // — CUSTOMERS —
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
                    if (!customersDict.TryGetValue(raw.CustomerName, out var cust))
                    {
                        cust = new ImportedCustomerViewModel
                        {
                            CustomerName = raw.CustomerName,
                            Email = raw.Email
                        };
                        customersDict.Add(raw.CustomerName, cust);
                    }

                    if (!string.IsNullOrEmpty(raw.Email) && !emailValidator.IsValid(raw.Email))
                    {
                        errors.Add(new ImportErrorViewModel
                        {
                            RowNumber = raw.RowNumber,
                            Message = $"Invalid email '{raw.Email}'."
                        });
                    }
                }

                // — SALES —
                if (!string.IsNullOrWhiteSpace(raw.ProductName) &&
                    !string.IsNullOrWhiteSpace(raw.CustomerName))
                {
                    DateTime? parsedDate = null;
                    if (!string.IsNullOrWhiteSpace(raw.SaleDateText))
                    {
                        if (DateTime.TryParse(raw.SaleDateText, out var d))
                        {
                            parsedDate = d;
                        }
                        else
                        {
                            errors.Add(new ImportErrorViewModel
                            {
                                RowNumber = raw.RowNumber,
                                Message = $"Invalid date '{raw.SaleDateText}'."
                            });
                        }
                    }

                    int? qty = null;
                    if (!string.IsNullOrWhiteSpace(raw.QuantityText))
                    {
                        if (int.TryParse(raw.QuantityText, out var q))
                        {
                            qty = q;
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

                    salesList.Add(new ImportedSaleViewModel
                    {
                        ProductName = raw.ProductName!,
                        CustomerName = raw.CustomerName!,
                        SaleDate = parsedDate,
                        Quantity = qty,
                        SourceRow = raw.RowNumber
                    });
                }
            }

            // llenar modelo para la vista
            model.TotalRows = rawRows.Count;
            model.ProductsCount = productsDict.Count;
            model.CustomersCount = customersDict.Count;
            model.SalesCount = salesList.Count;
            model.ErrorCount = errors.Count;
            model.Errors = errors;

            if (errors.Any())
            {
                TempData["ErrorMessage"] = $"Validation failed. {errors.Count} error(s) found. Nothing was saved.";
                return View(model);
            }

            // ===========================
            // DATABASE UPSERT
            // ===========================
            using var transaction = _context.Database.BeginTransaction();

            // — UPSERT PRODUCTS —
            var dbProducts = _context.Products.ToList()
                .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var p in productsDict.Values)
            {
                if (dbProducts.TryGetValue(p.ProductName, out var dbProd))
                {
                    if (p.Price.HasValue)
                        dbProd.UnitPrice = p.Price.Value;
                    dbProd.IsActive = true;
                }
                else
                {
                    var newp = new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = p.ProductName,
                        UnitPrice = p.Price ?? 0m,
                        Stock = 0,
                        IsActive = true
                    };
                    _context.Products.Add(newp);
                    dbProducts.Add(newp.Name, newp);
                }
            }

            // — UPSERT CUSTOMERS —
            var dbCustomers = _context.Customers.ToList()
                .ToDictionary(c => c.FullName, StringComparer.OrdinalIgnoreCase);

            foreach (var c in customersDict.Values)
            {
                if (dbCustomers.TryGetValue(c.CustomerName, out var dbCust))
                {
                    if (!string.IsNullOrWhiteSpace(c.Email))
                        dbCust.Email = c.Email!;
                    dbCust.IsActive = true;
                }
                else
                {
                    var newc = new Customer
                    {
                        Id = Guid.NewGuid(),
                        FullName = c.CustomerName,
                        Email = c.Email ?? "",
                        DocumentNumber = $"IMPORT-{Guid.NewGuid():N}",
                        PhoneNumber = "",
                        Age = 0,
                        IsActive = true
                    };
                    _context.Customers.Add(newc);
                    dbCustomers.Add(newc.FullName, newc);
                }
            }

            _context.SaveChanges();

            // — INSERT SALES —
            var taxRate = 0.19m;

            foreach (var s in salesList)
            {
                var product = dbProducts[s.ProductName];
                var customer = dbCustomers[s.CustomerName];
                var qty = s.Quantity ?? 1;

                var amount = product.UnitPrice * qty;
                var subtotal = amount;
                var tax = subtotal * taxRate;
                var total = subtotal + tax;

                var sale = new Sale
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,

                    // ✅ FECHA CONVERTIDA A UTC (SOLUCIÓN AL ERROR DEL OFFSET)
                    Date = s.SaleDate.HasValue
                        ? new DateTimeOffset(s.SaleDate.Value).ToUniversalTime()
                        : DateTimeOffset.UtcNow,

                    Subtotal = subtotal,
                    Tax = tax,
                    Total = total,
                    Status = SaleStatus.Confirmed,
                    Notes = "Imported from Excel"
                };

                sale.Items.Add(new SaleItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Quantity = qty,
                    UnitPrice = product.UnitPrice,
                    Amount = amount
                });

                _context.Sales.Add(sale);
            }

            _context.SaveChanges();
            transaction.Commit();

            TempData["SuccessMessage"] =
                $"Import successful! Products: {productsDict.Count}, Customers: {customersDict.Count}, Sales: {salesList.Count}.";

            return RedirectToAction(nameof(BulkImport));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] =
                $"ERROR: {ex.Message} | INNER: {ex.InnerException?.Message}";
            return RedirectToAction(nameof(BulkImport));
        }
    }
}
