    namespace Firmeza.Admin.Models;

    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
        
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
        
    }