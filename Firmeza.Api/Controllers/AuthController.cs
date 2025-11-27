using Firmeza.API.DTOs.Auth;
using Firmeza.Admin.Models;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using ApplicationUser = Firmeza.Infrastructure.Identity.ApplicationUser;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly FirmezaDbContext _db;
    private readonly IEmailService _emailService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        FirmezaDbContext db,
        IEmailService emailService)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
    }

    // =========================================
    // LOGIN
    // =========================================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and password are required.");

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return Unauthorized("Invalid credentials.");

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
            return Unauthorized("Invalid credentials.");

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
        var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");

        if (jwtKey is null || jwtIssuer is null || jwtAudience is null)
            return StatusCode(500, "JWT environment variables missing.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(3),
            signingCredentials: creds
        );

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }

    // =========================================
    // REGISTER (CLIENTE)
    // =========================================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
    {
        // 1. Validar email único
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing != null)
            return BadRequest("A user with this email already exists.");

        // 2. Crear IdentityUser
        var identityUser = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var userCreated = await _userManager.CreateAsync(identityUser, request.Password);

        if (!userCreated.Succeeded)
            return BadRequest(userCreated.Errors);

        // 3. Asignar rol Cliente
        await _userManager.AddToRoleAsync(identityUser, "Cliente");

        // 4. Guardar Customer en base de datos
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            DocumentNumber = request.DocumentNumber,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Age = request.Age,
            IsActive = true
        };

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        // 5. Enviar correo de bienvenida
        await _emailService.SendEmailAsync(
            request.Email,
            "Welcome to Firmeza",
            $"Hello {request.FullName}, your Firmeza account has been successfully created!"
        );

        // 6. Respuesta final
        return Ok(new
        {
            message = "Customer registered successfully",
            customerId = customer.Id
        });
    }
}
