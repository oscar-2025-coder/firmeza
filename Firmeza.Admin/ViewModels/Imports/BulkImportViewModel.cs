using System.ComponentModel.DataAnnotations;
namespace Firmeza.Admin.ViewModels.Imports;

public class BulkImportViewModel
{
      [Required(ErrorMessage = "Please select an Excel file.")]
            [Display(Name = "Excel file (.xlsx)")]
            public IFormFile? File { get; set; }
    
            // Later we can add:
            // - Summary of imported/updated records
            // - Error log entries
            // but for now we keep it simple.
}