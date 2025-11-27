namespace Firmeza.API.DTOs.Sales
{
    public class SaleUpdateDto
    {
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}