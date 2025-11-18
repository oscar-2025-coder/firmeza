using System.ComponentModel.DataAnnotations;
namespace Firmeza.Admin.ViewModels.Customers;

public class CustomerDeleteViewModel
{
    [Required]
    public Guid Id { get; set; }

    [Display(Name = "Full name")]
    public string? FullName { get; set; }
}