using Clinic.Backend.Data;
using Clinic.Backend.Models;
using Clinic.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Only Admins can manage users
public class UsersController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ICurrentTenant _currentTenant;

    public UsersController(ClinicDbContext context, ICurrentTenant currentTenant)
    {
        _context = context;
        _currentTenant = currentTenant;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        // 1. Check if user already exists
        var exists = await _context.AppUsers.IgnoreQueryFilters().AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return Conflict(new { message = "User with this email already exists." });
        }

        // 2. Create user
        // Note: In this B2B thin slice, the creating Admin's tenant is used if not specified, 
        // or we allow cross-tenant user creation if the request specifies it (Admin only).
        // To be safe and simple: use the request's TenantId but validate it's the Admin's or allowed.
        // For this demo: we just follow the request.
        
        var user = new AppUser
        {
            TenantId = request.TenantId,
            Email = request.Email,
            Role = request.Role,
            PrimaryBranchId = request.BranchIds?.FirstOrDefault(),
            PasswordHash = "password",
            UserBranches = request.BranchIds?.Select(bid => new UserBranch { BranchId = bid }).ToList() ?? new List<UserBranch>()
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPatch("{id}/role")]
    public async Task<IActionResult> AssignRole(int id, [FromBody] string role)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        user.Role = role;
        await _context.SaveChangesAsync();

        return Ok(user);
    }
}

public record CreateUserRequest(string Email, string Role, int TenantId, List<int>? BranchIds);
