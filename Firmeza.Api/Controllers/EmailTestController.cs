using Firmeza.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace Firmeza.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailTestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendTestEmail([FromQuery] string to)
    {
        if (string.IsNullOrWhiteSpace(to))
            return BadRequest("Destination email is required.");

        await _emailService.SendEmailAsync(
            to,
            "Email Test - Firmeza API",
            "<h2>Correo enviado correctamente desde Firmeza API ✔</h2><p>Este es un mensaje de prueba.</p>"
        );

        return Ok("Email sent successfully.");
    }
}