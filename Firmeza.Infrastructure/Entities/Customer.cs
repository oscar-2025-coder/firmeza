using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Entities
{
    [Index(nameof(DocumentNumber), IsUnique = true)]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(PhoneNumber), IsUnique = true)]
    public class Customer
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = default!;

        public string DocumentNumber { get; set; } = default!;

        public string Email { get; set; } = default!;

        public string PhoneNumber { get; set; } = default!;

        public int Age { get; set; }

        public bool IsActive { get; set; }

        // Relaciones (EF Core)
        public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    }
}