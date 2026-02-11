using System.ComponentModel.DataAnnotations;

namespace Clinic.Backend.Models;

public class Appointment
{
    public int Id { get; set; }
    
    [Required]
    public int TenantId { get; set; }
    
    [Required]
    public int BranchId { get; set; }
    
    [Required]
    public int PatientId { get; set; }
    
    [Required]
    public DateTime StartAt { get; set; }
    
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public Branch? Branch { get; set; }
    public Patient? Patient { get; set; }
}
