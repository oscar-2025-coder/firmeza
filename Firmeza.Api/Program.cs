using DotNetEnv;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// Load .env from the solution root (one level above Firmeza.Api)
// -----------------------------------------------------------
Env.Load("../.env");

// -----------------------------------------------------------
// Read DB and JWT variables from environment
// -----------------------------------------------------------
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// -----------------------------------------------------------
// DEBUG PRINT – THIS IS WHAT WE NEED TO SEE IN THE CONSOLE
// -----------------------------------------------------------
Console.WriteLine("ENV VALUE (DB_CONNECTION): " + dbConnectionString);
Console.WriteLine("JWT_KEY VALUE: " + jwtKey);
Console.WriteLine("JWT_ISSUER VALUE: " + jwtIssuer);
Console.WriteLine("JWT_AUDIENCE VALUE: " + jwtAudience);

// -----------------------------------------------------------
// Register DbContext
// -----------------------------------------------------------
builder.Services.AddDbContext<FirmezaDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString);
});

// -----------------------------------------------------------
// Register Identity
// -----------------------------------------------------------
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<FirmezaDbContext>()
.AddDefaultTokenProviders();

// -----------------------------------------------------------
// Register Authentication with JWT
// -----------------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// -----------------------------------------------------------
// Authorization
// -----------------------------------------------------------
builder.Services.AddAuthorization();

// -----------------------------------------------------------
// Controllers & Swagger
// -----------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -----------------------------------------------------------
// Swagger
// -----------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// -----------------------------------------------------------
// Middleware pipeline
// -----------------------------------------------------------
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------
// TEMP weather endpoint
// -----------------------------------------------------------
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy",
        "Hot", "Sweltering", "Scorching"
    };

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();

    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

// -----------------------------------------------------------
// Seed roles
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.Run();

// -----------------------------------------------------------
// temp record (delete later)
// -----------------------------------------------------------
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
