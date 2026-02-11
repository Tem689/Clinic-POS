using System.ComponentModel.DataAnnotations;

namespace Clinic.Backend.Models;

public class AppUser
{
    public int Id { get; set; }

    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "Viewer"; // Admin, User, Viewer

    public int? PrimaryBranchId { get; set; }
    public Branch? PrimaryBranch { get; set; }

    public ICollection<UserBranch> UserBranches { get; set; } = new List<UserBranch>();

    public string PasswordHash { get; set; } = string.Empty; 
}
