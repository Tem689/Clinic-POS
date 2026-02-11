using Clinic.Backend.Data;
using Clinic.Backend.Events;
using Clinic.Backend.Models;
using Clinic.Backend.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly ClinicDbContext _context;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPublishEndpoint _publishEndpoint;

    public AppointmentsController(
        ClinicDbContext context, 
        ICurrentTenant currentTenant,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _currentTenant = currentTenant;
        _publishEndpoint = publishEndpoint;
    }

    [HttpPost]
    [Authorize(Policy = "CanCreateAppointments")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest request)
    {
        // 1. Validate if patient exists in same tenant
        var patient = await _context.Patients.AnyAsync(p => p.Id == request.PatientId);
        if (!patient) return BadRequest("Patient not found in current tenant.");

        // 2. Check for duplicate (same patient, same time, same branch) -> Section C2
        // Manual check for InMemory support and extra safety
        var exists = await _context.Appointments.AnyAsync(a => 
            a.PatientId == request.PatientId && 
            a.StartAt == request.StartAt && 
            a.BranchId == request.BranchId);
            
        if (exists) return Conflict(new { message = "Duplicate booking detected for this patient at the same time and branch." });

        var appointment = new Appointment
        {
            TenantId = _currentTenant.Id!.Value,
            BranchId = request.BranchId,
            PatientId = request.PatientId,
            StartAt = request.StartAt,
            CreatedAt = DateTime.UtcNow
        };

        try 
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Requirement C2: Friendly error on duplicate
            return Conflict(new { message = "Duplicate booking detected for this patient at the same time and branch." });
        }

        // 3. Publish Event (Requirement C3)
        await _publishEndpoint.Publish(new AppointmentCreated(
            appointment.Id,
            appointment.TenantId,
            appointment.BranchId,
            appointment.PatientId,
            appointment.StartAt));

        return CreatedAtAction(nameof(CreateAppointment), new { id = appointment.Id }, appointment);
    }
}

public record CreateAppointmentRequest(int PatientId, int BranchId, DateTime StartAt);
