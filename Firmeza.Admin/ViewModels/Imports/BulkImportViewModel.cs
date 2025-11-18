using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Firmeza.Admin.ViewModels.Imports
{
    public class BulkImportViewModel
    {
        [Required(ErrorMessage = "Please select an Excel file.")]
        [Display(Name = "Excel file (.xlsx)")]
        public IFormFile? File { get; set; }

        public int TotalRows { get; set; }
        public int ProductsCount { get; set; }
        public int CustomersCount { get; set; }
        public int SalesCount { get; set; }
        public int ErrorCount { get; set; }

        public List<ImportErrorViewModel> Errors { get; set; } = new();
    }
}