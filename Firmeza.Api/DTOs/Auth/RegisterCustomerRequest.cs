namespace Firmeza.API.DTOs.Auth
{
    public class RegisterCustomerRequest
    {
        public string FullName { get; set; } = default!;
        public string DocumentNumber { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public int Age { get; set; }
        public string Password { get; set; } = default!;
    }
}