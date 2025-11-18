namespace Firmeza.Admin.ViewModels.Imports
{
    public class ImportedProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal? Price { get; set; }
        public List<int> SourceRows { get; set; } = new();
    }
}