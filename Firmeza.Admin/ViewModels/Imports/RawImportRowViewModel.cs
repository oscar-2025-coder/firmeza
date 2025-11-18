namespace Firmeza.Admin.ViewModels.Imports
{
    /// <summary>
    /// Represents one raw row from the Excel file (denormalized data).
    /// </summary>
    public class RawImportRowViewModel
    {
        public int RowNumber { get; set; }

        // Product-related columns
        public string? ProductName { get; set; }
        public string? PriceText { get; set; }

        // Customer-related columns
        public string? CustomerName { get; set; }
        public string? Email { get; set; }

        // Sale-related columns
        public string? SaleDateText { get; set; }
        public string? QuantityText { get; set; }
    }
}