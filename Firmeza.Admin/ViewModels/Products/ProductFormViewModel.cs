using System;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Admin.ViewModels.Products
{
    public class ProductFormViewModel
    {
        public Guid Id { get; set; }

        [Required, StringLength(200, MinimumLength = 2)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "SKU")]
        [RegularExpression("^[A-Z0-9_-]+$", ErrorMessage = "Use only A-Z, 0-9, dash or underscore.")]
        public string? Sku { get; set; }

        [Range(typeof(decimal), "0", "9999999,99", ErrorMessage = "Enter a valid price between 0 and 9,999,999.99")]
        [Display(Name = "Unit Price")]
        [DataType(DataType.Currency)]
        public decimal UnitPrice { get; set; }

        [Range(0, 1000000, ErrorMessage = "Stock must be between 0 and 1,000,000")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // ==========================================================
        // Duplicate SKU handling
        // ==========================================================

        /// <summary>
        /// ID of the existing product if a duplicate SKU is found.
        /// </summary>
        public Guid? ExistingProductId { get; set; }

        /// <summary>
        /// Message to show in the Create view when SKU is duplicated.
        /// </summary>
        public string? DuplicateMessage { get; set; }
    }
}