
using Microsoft.AspNetCore.Identity;
using DotNetEnv;
using Firmeza.Admin.Models;
using Microsoft.EntityFrameworkCore;
using Firmeza.Admin.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ApplicationDbContextConnection") ?? throw new InvalidOperationException("Connection string 'ApplicationDbContextConnection' not found.");

// update .env from main solution 
Env.Load("../.env");

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new NullReferenceException("DB_CONNECTION environment variable not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<ApplicationDbContext>();

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


builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = "/Identity/Account/Login";
    opt.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddRazorPages();


// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication(); 


app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
await Firmeza.Admin.Identity.IdentitySeeder.SeedAsync(app.Services);
app.Run();