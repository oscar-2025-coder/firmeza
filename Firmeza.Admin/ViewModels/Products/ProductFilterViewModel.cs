using System.ComponentModel.DataAnnotations;

namespace Firmeza.Admin.ViewModels.Products
{
    public class ProductFilterViewModel
    {
        // Texto libre de búsqueda (Name, SKU, Description)
        [Display(Name = "Search")]
        public string? Q { get; set; }

        // Filtro por rango de precios
        [Range(typeof(decimal), "0", "9999999.99")]
        [Display(Name = "Min Price")]
        public decimal? MinPrice { get; set; }

        [Range(typeof(decimal), "0", "9999999.99")]
        [Display(Name = "Max Price")]
        public decimal? MaxPrice { get; set; }

        // Filtro por estado (activos/inactivos)
        [Display(Name = "Only Active")]
        public bool? OnlyActive { get; set; }
    }
}