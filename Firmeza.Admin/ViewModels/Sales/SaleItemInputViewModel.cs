using System;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Admin.ViewModels.Sales
{
    public class SaleItemInputViewModel
    {
        [Required]
        public Guid ProductId { get; set; }

        // This is optional, mainly for display purposes in the view
        public string? ProductName { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        public decimal UnitPrice { get; set; }
    }
}