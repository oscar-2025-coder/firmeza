using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Firmeza.Admin.ViewModels.Sales
{
    public class SaleCreateViewModel
    {
        [Required]
        [Display(Name = "Customer")]
        public Guid CustomerId { get; set; }

        // Dropdown lists for the form
        public List<SelectListItem> Customers { get; set; } = new();
        public List<SelectListItem> Products { get; set; } = new();

        // Items in the sale
        public List<SaleItemInputViewModel> Items { get; set; } = new();

        // Optional notes
        public string? Notes { get; set; }

        // Calculated totals (we will compute them on the server side)
        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
    }
}