using Microsoft.EntityFrameworkCore;
namespace Firmeza.Admin.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}