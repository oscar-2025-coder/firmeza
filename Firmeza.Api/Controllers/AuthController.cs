using Firmeza.API.DTOs.Auth;
using Firmeza.Infrastructure.Data;
using Firmeza.Infrastructure.Entities;
using Firmeza.Infrastructure.Identity;
using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Firmeza.API.Controllers
{
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

        // ============================================================
        // LOGIN
        // ============================================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Email and password are required.");

            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials.");

            var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!validPassword)
                return Unauthorized("Invalid credentials.");

            // Obtener el Customer asociado
            var customer = await _db.Customers
                .FirstOrDefaultAsync(c => c.Email == user.Email);

            if (customer == null)
                return BadRequest("Customer does not exist.");

            // Claims
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),

                // IMPORTANTÍSIMO PARA EL FRONT
                new Claim("customerId", customer.Id.ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // JWT
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

        // ============================================================
        // REGISTER
        // ============================================================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCustomerRequest request)
        {
            var existing = await _userManager.FindByEmailAsync(request.Email);
            if (existing != null)
                return BadRequest("A user with this email already exists.");

            var identityUser = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email
            };

            var created = await _userManager.CreateAsync(identityUser, request.Password);
            if (!created.Succeeded)
                return BadRequest(created.Errors);

            await _userManager.AddToRoleAsync(identityUser, "Cliente");

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

            await _emailService.SendEmailAsync(
                request.Email,
                "Welcome to Firmeza",
                $"Hello {request.FullName}, your account has been created!"
            );

            return Ok(new
            {
                message = "Customer registered successfully",
                customerId = customer.Id
            });
        }
    }
}
