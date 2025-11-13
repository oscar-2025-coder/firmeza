using System.ComponentModel.DataAnnotations;

namespace Firmeza.Admin.ViewModels.Customers
{
    public class CustomerCreateViewModel
    {
        [Required]
        [MaxLength(150)]
        [Display(Name = "Full name")]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required]
        [MaxLength(50)]
        [Display(Name = "Document number")]
        public string? DocumentNumber { get; set; }

        [Required]
        [Phone]
        [MaxLength(30)]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        // Age provided as text (to test int.Parse with try-catch)
        [Display(Name = "Age")]
        public string? AgeText { get; set; }

        // Parsed age if conversion succeeds
        public int? Age { get; set; }

        // Friendly error message for invalid input
        public string? ErrorMessage { get; set; }
    }
}