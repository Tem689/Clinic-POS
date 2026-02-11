using Clinic.Backend.Data;
using Clinic.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthController(ClinicDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Simple mock login: in real app, we verify PasswordHash
        var user = await _context.AppUsers
            .IgnoreQueryFilters() // Auth needs to check all users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.PasswordHash == request.Password);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        var token = _tokenService.GenerateToken(user.Id, user.TenantId, user.Email, user.Role);

        return Ok(new
        {
            token,
            user = new { user.Email, user.Role, user.TenantId }
        });
    }
}

public record LoginRequest(string Email, string Password);
