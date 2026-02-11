using System.ComponentModel.DataAnnotations;

namespace Clinic.Backend.Models;

public class Branch
{
    public int Id { get; set; }

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
