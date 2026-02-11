using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Backend.Models;

[Index(nameof(TenantId), nameof(PhoneNumber), IsUnique = true)]
public class Patient
{
    public int Id { get; set; }

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public int? PrimaryBranchId { get; set; }
    public Branch? PrimaryBranch { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
