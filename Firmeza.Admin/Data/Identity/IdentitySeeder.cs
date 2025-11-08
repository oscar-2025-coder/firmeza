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
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure roles exist
            if (!await roleManager.RoleExistsAsync(RoleAdministrator))
                await roleManager.CreateAsync(new IdentityRole(RoleAdministrator));

            if (!await roleManager.RoleExistsAsync(RoleCustomer))
                await roleManager.CreateAsync(new IdentityRole(RoleCustomer));

            // Ensure an admin user exists
            var adminEmail = "admin@firmeza.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");
                await userManager.AddToRoleAsync(adminUser, RoleAdministrator);
            }
        }
    }
}