namespace Firmeza.API.DTOs.Products;

public class ProductCreateDto
{
    public string Name { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
    public string? Sku { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
}