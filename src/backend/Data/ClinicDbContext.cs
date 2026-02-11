using Clinic.Backend.Models;
using Clinic.Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Clinic.Backend.Data;

public class ClinicDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;

    public ClinicDbContext(DbContextOptions<ClinicDbContext> options, ICurrentTenant currentTenant) : base(options)
    {
        _currentTenant = currentTenant;
    }

    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<AppUser> AppUsers { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<UserBranch> UserBranches { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserBranch Junction Table
        modelBuilder.Entity<UserBranch>()
            .HasKey(ub => new { ub.AppUserId, ub.BranchId });

        modelBuilder.Entity<UserBranch>()
            .HasOne(ub => ub.AppUser)
            .WithMany(u => u.UserBranches)
            .HasForeignKey(ub => ub.AppUserId);

        modelBuilder.Entity<UserBranch>()
            .HasOne(ub => ub.Branch)
            .WithMany()
            .HasForeignKey(ub => ub.BranchId);

        // Global Query Filter for Tenant Isolation
        // If TenantId is set in the context, filter by it.
        // For Patients, Branches, AppUsers.
        // NOTE: Tenants themselves might be global or not? Usually global lookup, but let's filters them too if needed?
        // Actually, Tenants table is usually global.
        
        // Filter Patients
        modelBuilder.Entity<Patient>().HasQueryFilter(p => _currentTenant.Id == null || p.TenantId == _currentTenant.Id);
        
        // Filter Branches (optional but good)
        modelBuilder.Entity<Branch>().HasQueryFilter(b => _currentTenant.Id == null || b.TenantId == _currentTenant.Id);

        // Filter AppUsers (users belong to tenant)
        modelBuilder.Entity<AppUser>().HasQueryFilter(u => _currentTenant.Id == null || u.TenantId == _currentTenant.Id);

        // Relationships
        modelBuilder.Entity<Branch>()
            .HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AppUser>()
            .HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Patient>()
            .HasOne(p => p.PrimaryBranch)
            .WithMany()
            .HasForeignKey(p => p.PrimaryBranchId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
