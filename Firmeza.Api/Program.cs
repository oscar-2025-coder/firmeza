using DotNetEnv;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// ============================================================
// Load .env ONLY in local environment
// ============================================================
try
{
    Env.Load("../.env");
}
catch
{
    Console.WriteLine("⚠️ .env file not found. Continuing...");
}

// ============================================================
// Read environment variables
// ============================================================
var dbConnectionEnv = Environment.GetEnvironmentVariable("DB_CONNECTION");
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

// ============================================================
// Build final connection string
// Priority:
// 1. DB_CONNECTION (Docker)
// 2. appsettings.json (Local development)
// ============================================================
var connectionString = dbConnectionEnv;

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("❌ ERROR: No database connection string found. Set DB_CONNECTION or DefaultConnection.");
}

// ============================================================
// DbContext
// ============================================================
builder.Services.AddDbContext<FirmezaDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// ============================================================
// Identity
// ============================================================
builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;
})
.AddRoles<ApplicationRole>()
.AddEntityFrameworkStores<FirmezaDbContext>()
.AddDefaultTokenProviders();

// ============================================================
// JWT Authentication
// ============================================================
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

// ============================================================
// Authorization
// ============================================================
builder.Services.AddAuthorization();

// ============================================================
// Controllers + AutoMapper
// ============================================================
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ============================================================
// CORS (Allow Frontend)
// ============================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============================================================
// Email Service (SMTP)
// ============================================================
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPdfService, PdfService>();


// ============================================================
// Swagger + JWT Support
// ============================================================
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
        Scheme = "bearer",
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

// ============================================================
// Build application
// ============================================================
var app = builder.Build();

// ============================================================
// Swagger ALWAYS enabled
// ============================================================
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.DisplayRequestDuration();
    options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
});

// ============================================================
// Pipeline
// ============================================================
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ============================================================
// Seed Roles (Admin, Cliente)
// ============================================================
using (var scope = app.Services.CreateScope())
{
    // Seed roles on startup - commented out to prevent DNS crash
    // RoleSeeder.SeedRolesAsync(scope.ServiceProvider);
}

app.Run();
