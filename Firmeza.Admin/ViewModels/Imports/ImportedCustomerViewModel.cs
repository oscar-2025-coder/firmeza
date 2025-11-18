namespace Firmeza.Admin.ViewModels.Imports
{
    public class ImportedCustomerViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public List<int> SourceRows { get; set; } = new();
    }
}