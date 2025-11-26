namespace Firmeza.API.DTOs.Sales;

public class SaleItemCreateDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}