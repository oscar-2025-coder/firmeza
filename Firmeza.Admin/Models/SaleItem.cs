namespace Firmeza.admi.Models;

public class SaleItem
{
    public Guid Id { get; set; }
    public Guid SaledId { get; set; }
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    public Sale Sale { get; set; } = default!;
    public Product Product { get; set; } = default!;
}