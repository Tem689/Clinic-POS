using System.ComponentModel.DataAnnotations;

namespace Clinic.Backend.Models;

public class Tenant
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}
