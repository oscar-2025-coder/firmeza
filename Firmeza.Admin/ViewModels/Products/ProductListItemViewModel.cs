
namespace Firmeza.Admin.ViewModels.Products
{
    public class ProductListItemViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public decimal UnitPrice { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}