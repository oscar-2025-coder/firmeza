namespace Firmeza.Admin.ViewModels.Imports
{
    public class ImportedSaleViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? SaleDate { get; set; }
        public int? Quantity { get; set; }
        public int SourceRow { get; set; }
    }
}