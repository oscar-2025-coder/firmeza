namespace Firmeza.admi.Models;

public class Sale
{
    // Properties
    public Guid Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public Guid CustomerId { get; set; }
    
    // Navigation Properties
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    
    // Enum for Sale Status
    public SaleStatus Status { get; set; }
    public string? Notes { get; set; }
    
    // navigation property to Customer
    public Customer Customer { get; set; } = default!;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
}