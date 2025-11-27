using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Firmeza.Infrastructure.Identity;

// Seeder to create required roles for the API
public static class RoleSeeder
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        // Required roles for the system
        string[] roles = { "Administrador", "Cliente" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new ApplicationRole 
                { 
                    Name = roleName,
                    NormalizedName = roleName.ToUpper()
                });
            }
        }
    }
}