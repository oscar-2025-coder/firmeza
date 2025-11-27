using DotNetEnv;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Services;   // 👈 IMPORTANTE: EmailService
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------------------------
// Load .env
// -----------------------------------------------------------
Env.Load("../.env");

// -----------------------------------------------------------
// Environment variables
// -----------------------------------------------------------
var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// -----------------------------------------------------------
// DbContext
// -----------------------------------------------------------
builder.Services.AddDbContext<FirmezaDbContext>(options =>
{
    options.UseNpgsql(dbConnectionString);
});

// -----------------------------------------------------------
// Identity
// -----------------------------------------------------------
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<FirmezaDbContext>()
.AddDefaultTokenProviders();

// -----------------------------------------------------------
// JWT Authentication
// -----------------------------------------------------------
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

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
// Controllers + AutoMapper
// -----------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// -----------------------------------------------------------
// Email Service (SMTP Gmail)
// -----------------------------------------------------------
builder.Services.AddScoped<IEmailService, EmailService>();   // 👈 REGISTRO OFICIAL

// -----------------------------------------------------------
// Swagger + JWT
// -----------------------------------------------------------
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Firmeza.Api",
        Version = "v1"
    });

    var securitySchema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter: **Bearer {your JWT token}**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // lowercase required
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };

    c.AddSecurityDefinition("Bearer", securitySchema);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securitySchema, Array.Empty<string>() }
    });
});

// -----------------------------------------------------------
// Build app
// -----------------------------------------------------------
var app = builder.Build();

// -----------------------------------------------------------
// Swagger ENABLED ALWAYS
// -----------------------------------------------------------
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DisplayRequestDuration();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
});

// -----------------------------------------------------------
// Pipeline
// -----------------------------------------------------------
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// -----------------------------------------------------------
// Seed Roles (Admin, Cliente)
// -----------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    await RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.Run();
