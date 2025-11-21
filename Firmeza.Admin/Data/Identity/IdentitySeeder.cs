using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Firmeza.Admin.Models;

namespace Firmeza.Admin.Identity
{
    public static class IdentitySeeder
    {
        public const string RoleAdministrator = "Administrador";
        public const string RoleCustomer = "Customer";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure roles exist
            if (!await roleManager.RoleExistsAsync(RoleAdministrator))
                await roleManager.CreateAsync(new IdentityRole(RoleAdministrator));

            if (!await roleManager.RoleExistsAsync(RoleCustomer))
                await roleManager.CreateAsync(new IdentityRole(RoleCustomer));

            // ❌ User creation removed (must be created via UI)
        }
    }
}