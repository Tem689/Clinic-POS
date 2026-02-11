using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = "CanCreateAppointments")]
    public IActionResult CreateAppointment()
    {
        // Stubbed for Section B2 demonstration
        return Ok(new { message = "Appointment creation permission verified." });
    }
}
