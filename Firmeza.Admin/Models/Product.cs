namespace Firmeza.Admin.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        // Core fields
        public string Name { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }

        // New fields
        // SKU is optional for now; we can enforce uniqueness later with an index.
        public string? Sku { get; set; }

        // Default to 0 at the database level in the migration (we'll add it).
        public int Stock { get; set; }

        // Optional free text
        public string? Description { get; set; }

        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}