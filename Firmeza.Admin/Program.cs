using DotNetEnv;
using Firmeza.Admin.Data;
using Firmeza.Admin.Models;
using Firmeza.Admin.Services.Pdf;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF license (Community)
QuestPDF.Settings.License = LicenseType.Community;

// Load environment variables (.env)
Env.Load("../.env");

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
                       ?? throw new NullReferenceException("DB_CONNECTION environment variable not found");

// DbContext (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity with roles + default UI
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

// Authentication cookies
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Identity/Account/Login";
    opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// MVC + Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// PDF services
builder.Services.AddTransient<ReceiptPdfService>();

var app = builder.Build();

// HTTP pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

// Redirect admin (ES/EN) from "/" to "/Admin/Index"
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

app.UseAuthorization();

// Endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seeder (roles + admin)
using (var scope = app.Services.CreateScope())
{
    await Firmeza.Admin.Identity.IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();
