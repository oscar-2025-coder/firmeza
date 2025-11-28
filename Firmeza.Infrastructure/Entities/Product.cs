using System;
using System.Collections.Generic;

namespace Firmeza.Infrastructure.Entities
{
    public class Product
    {
        public Guid Id { get; set; }

        // Core fields
        public string Name { get; set; } = default!;
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }

        // SKU optional
        public string? Sku { get; set; }

        // Stock defaulted to 0
        public int Stock { get; set; }

        // Optional description
        public string? Description { get; set; }

        // Relaciones EF Core
        public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    }
}