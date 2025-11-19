namespace Firmeza.Admin.Models;

public class Sale
{
    // Properties
    public Guid Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public Guid CustomerId { get; set; }

    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    // Enum for Sale Status
    public SaleStatus Status { get; set; }
    public string? Notes { get; set; }

    // PDF receipt file name stored under wwwroot/receipts
    public string? ReceiptFileName { get; set; }

    // Navigation properties
    public Customer Customer { get; set; } = default!;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}