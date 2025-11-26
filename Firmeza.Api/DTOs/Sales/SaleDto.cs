namespace Firmeza.API.DTOs.Sales;

public class SaleDto
{
    public Guid Id { get; set; }
    public DateTimeOffset Date { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = default!;
    public string? Notes { get; set; }
    public string? ReceiptFileName { get; set; }

    public List<SaleItemDto> Items { get; set; } = new();
}