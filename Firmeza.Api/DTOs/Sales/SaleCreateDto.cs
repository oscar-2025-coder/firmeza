namespace Firmeza.API.DTOs.Sales;

public class SaleCreateDto
{
    public Guid CustomerId { get; set; }

    public List<SaleItemCreateDto> Items { get; set; } = new();

    public string? Notes { get; set; }
}