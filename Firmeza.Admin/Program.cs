using DotNetEnv;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.Services.Pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// 1. Licencia PDF
QuestPDF.Settings.License = LicenseType.Community;

// 2. Cargar Variables de Entorno
Env.Load("../.env");

// 3. Configurar Cultura
var culture = new CultureInfo("es-CO");
culture.NumberFormat.NumberDecimalSeparator = ",";
culture.NumberFormat.NumberGroupSeparator = ".";
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// 4. Conexión a Base de Datos
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? builder.Configuration.GetConnectionString("ApplicationDbContextConnection")
                       ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 5. Configuración de Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// 6. Cookies
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Identity/Account/Login";
    opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 7. MVC, Razor y PDF
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<ReceiptPdfService>();

var app = builder.Build();

// ---- PIPELINE ----

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Redirección Admin
app.Use(async (context, next) =>
{
    var isRoot = context.Request.Path == "/";
    var isAuthenticated = context.User?.Identity?.IsAuthenticated == true;
    var isAdminEn = context.User?.IsInRole("Administrator") == true;
    var isAdminEs = context.User?.IsInRole("Administrador") == true;

    if (isRoot && isAuthenticated && (isAdminEn || isAdminEs))
    {
        context.Response.Redirect("/Admin/Index");
        return;
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// 8. 🚫 DESACTIVADO EL SEEDER (ROMPE NEON)
// NO USAR EN PRODUCCIÓN
// using (var scope = app.Services.CreateScope())
// {
//     await Firmeza.Admin.Identity.IdentitySeeder.SeedAsync(scope.ServiceProvider);
// }

app.Run();
