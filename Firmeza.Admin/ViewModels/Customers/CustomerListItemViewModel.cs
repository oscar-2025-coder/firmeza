namespace Firmeza.Admin.ViewModels.Customers;

public class CustomerListItemViewModel
{
    // Same type as in Customer model
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;
}