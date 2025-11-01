using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Firmeza.Admin.Data;
var builder = WebApplication.CreateBuilder(args);

// update .env from main solution 
Env.Load("../.env");

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
    ?? throw new NullReferenceException("DB_CONNECTION environment variable not found");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();