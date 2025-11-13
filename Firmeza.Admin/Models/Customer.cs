namespace Firmeza.Admin.Models
{
    public class Customer
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = default!;

        public string DocumentNumber { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;

        public int Age { get; set; }

        public bool IsActive { get; set; }

        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}