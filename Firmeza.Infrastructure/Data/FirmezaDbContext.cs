using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.Infrastructure.Data;

// Main DbContext including Identity tables
public class FirmezaDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public FirmezaDbContext(DbContextOptions<FirmezaDbContext> options)
        : base(options)
    {
    }

    // Entities for the API will be added later here
}