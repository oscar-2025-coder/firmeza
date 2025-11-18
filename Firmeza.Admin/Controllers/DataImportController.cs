using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Firmeza.Admin.ViewModels.Imports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace Firmeza.Admin.Controllers;

[Authorize(Roles = "Administrator,Administrador")]
public class DataImportController : Controller
{
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
                        return RedirectToAction(nameof(BulkImport));
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

                    // ============ NORMALIZE DATA (PRODUCTS, CUSTOMERS, SALES) ============

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

                    TempData["SuccessMessage"] =
                        $"Columns detected: {string.Join(", ", headers)}. " +
                        $"Rows read: {rawRows.Count}. " +
                        $"Products found: {products.Count}. " +
                        $"Customers found: {customers.Count}. " +
                        $"Sales found: {sales.Count}. " +
                        $"Errors found: {errorCount}. " +
                        "(Data normalized and validated in memory, not saved to database yet.)";

                    // Más adelante podremos mostrar 'errors' en la vista o guardarlo en algún lugar.
                }
            }
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An error occurred while reading the Excel file.";
        }

        return RedirectToAction(nameof(BulkImport));
    }
}
