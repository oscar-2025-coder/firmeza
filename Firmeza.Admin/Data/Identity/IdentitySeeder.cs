using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Firmeza.Admin.Models;

namespace Firmeza.Admin.Identity
{
    public static class IdentitySeeder
    {
        public const string RoleAdmin = "Administrador";
        public const string RoleCliente = "Cliente";

        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1️⃣ Crear roles si no existen
            if (!await roleManager.RoleExistsAsync(RoleAdmin))
                await roleManager.CreateAsync(new IdentityRole(RoleAdmin));

            if (!await roleManager.RoleExistsAsync(RoleCliente))
                await roleManager.CreateAsync(new IdentityRole(RoleCliente));

            // 2️⃣ Crear un usuario administrador de ejemplo
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
                await userManager.AddToRoleAsync(adminUser, RoleAdmin);
            }
        }
    }
}