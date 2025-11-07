using System.ComponentModel.DataAnnotations.Schema;

namespace Firmeza.Admin.Models;

public class SaleItem
{
    public Guid Id { get; set; }
    
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }

    public Sale Sale { get; set; } = default!;
    public Product Product { get; set; } = default!;
}