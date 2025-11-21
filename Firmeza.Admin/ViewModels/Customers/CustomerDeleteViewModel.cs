using System;
using System.ComponentModel.DataAnnotations;

namespace Firmeza.Admin.ViewModels.Customers
{
    public class CustomerDeleteViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Display(Name = "Full name")]
        public string FullName { get; set; } = string.Empty;

        // 🔥 Necesaria para evitar errores y mostrar advertencia en la vista
        public bool HasRelatedSales { get; set; }
    }
}