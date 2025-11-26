namespace Firmeza.API.DTOs.Customers;

public class CustomerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string DocumentNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
}