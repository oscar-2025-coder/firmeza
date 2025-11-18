using System.ComponentModel.DataAnnotations;
namespace Firmeza.Admin.ViewModels.Customers;

public class CustomerEditViewModel
{
    [Required]
    public Guid Id { get; set; }

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

    [Required]
    [Range(0, 120, ErrorMessage = "Age must be between 0 and 120.")]
    [Display(Name = "Age")]
    public int Age { get; set; }

    [Display(Name = "Is active")]
    public bool IsActive { get; set; }
}