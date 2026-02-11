using System.Text.Json;
using Clinic.Backend.Data;
using Clinic.Backend.Events;
using Clinic.Backend.Models;
using Clinic.Backend.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Clinic.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;

    public PatientsController(
        ClinicDbContext context, 
        ICurrentTenant currentTenant, 
        IDistributedCache cache,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _currentTenant = currentTenant;
        _cache = cache;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewPatients")]
    public async Task<IActionResult> GetPatients([FromQuery] int? branchId)
    {
        // Cache Key: tenant:{tid}:patients:list:{branchId|all}
        var cacheKey = $"tenant:{_currentTenant.Id}:patients:list:{(branchId?.ToString() ?? "all")}";

        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedData))
        {
            return Ok(JsonSerializer.Deserialize<List<Patient>>(cachedData));
        }

        var query = _context.Patients.AsQueryable();

        if (branchId.HasValue)
        {
            query = query.Where(p => p.PrimaryBranchId == branchId.Value);
        }

        var patients = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        // Set Cache
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(patients), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        return Ok(patients);
    }

    [HttpPost]
    [Authorize(Policy = "CanCreatePatients")]
    public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
    {
        // 1. Check uniqueness (phoneNumber within tenant)
        // Global query filter ensures we only check the current tenant's patients.
        var exists = await _context.Patients.AnyAsync(p => p.PhoneNumber == request.PhoneNumber);
        if (exists)
        {
            return Conflict(new { message = "Patient with this phone number already exists in this tenant." });
        }

        var patient = new Patient
        {
            TenantId = _currentTenant.Id!.Value,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            PrimaryBranchId = request.PrimaryBranchId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        // 2. Invalidate Cache
        // Simple invalidation: clear all list keys for this tenant or at least the 'all' and specific branch
        await _cache.RemoveAsync($"tenant:{_currentTenant.Id}:patients:list:all");
        if (request.PrimaryBranchId.HasValue)
        {
            await _cache.RemoveAsync($"tenant:{_currentTenant.Id}:patients:list:{request.PrimaryBranchId}");
        }

        // 3. Publish Event
        await _publishEndpoint.Publish(new PatientCreated(
            patient.Id, 
            patient.TenantId, 
            $"{patient.FirstName} {patient.LastName}"));

        return CreatedAtAction(nameof(GetPatients), new { id = patient.Id }, patient);
    }
}

public record CreatePatientRequest(string FirstName, string LastName, string PhoneNumber, int? PrimaryBranchId);
