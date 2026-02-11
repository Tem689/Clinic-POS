namespace Clinic.Backend.Models;

public class UserBranch
{
    public int AppUserId { get; set; }
    public AppUser? AppUser { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }
}
