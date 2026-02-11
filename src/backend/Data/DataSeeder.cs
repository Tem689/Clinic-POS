using Clinic.Backend.Data;
using Clinic.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Backend.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ClinicDbContext context)
    {
        if (await context.Tenants.AnyAsync()) return;

        // 1. Seed Tenant
        var tenant = new Tenant { Name = "Main Clinic Group" };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // 2. Seed Branches
        var branch1 = new Branch { TenantId = tenant.Id, Name = "Siam Branch" };
        var branch2 = new Branch { TenantId = tenant.Id, Name = "Sukhumvit Branch" };
        context.Branches.AddRange(branch1, branch2);
        await context.SaveChangesAsync();

        // 3. Seed Users
        // Password hashing skipped for simplicity as per stub allowance, 
        // but role-based auth remains functional via JWT.
        var admin = new AppUser 
        { 
            TenantId = tenant.Id, 
            Email = "admin@clinic.com", 
            Role = "Admin", 
            PasswordHash = "password",
            UserBranches = new List<UserBranch> 
            { 
                new UserBranch { BranchId = branch1.Id },
                new UserBranch { BranchId = branch2.Id }
            }
        };
        var user = new AppUser 
        { 
            TenantId = tenant.Id, 
            Email = "user@clinic.com", 
            Role = "User", 
            PasswordHash = "password",
            UserBranches = new List<UserBranch> 
            { 
                new UserBranch { BranchId = branch1.Id }
            }
        };
        var viewer = new AppUser 
        { 
            TenantId = tenant.Id, 
            Email = "viewer@clinic.com", 
            Role = "Viewer", 
            PasswordHash = "password",
            UserBranches = new List<UserBranch> 
            { 
                new UserBranch { BranchId = branch1.Id }
            }
        };

        context.AppUsers.AddRange(admin, user, viewer);
        await context.SaveChangesAsync();
    }
}
