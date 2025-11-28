using System;
using System.Collections.Generic;

namespace Firmeza.Infrastructure.Entities
{
    public class Sale
    {
        public Guid Id { get; set; }
        public DateTimeOffset Date { get; set; }
        public Guid CustomerId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }

        public SaleStatus Status { get; set; }
        public string? Notes { get; set; }

        // Nombre del archivo PDF (se guarda en wwwroot/receipts)
        public string? ReceiptFileName { get; set; }

        // Relaciones EF Core
        public Customer Customer { get; set; } = default!;
        public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }
}