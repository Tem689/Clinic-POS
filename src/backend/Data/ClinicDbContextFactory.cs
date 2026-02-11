using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clinic.Backend.Data;

public class ClinicDbContextFactory : IDesignTimeDbContextFactory<ClinicDbContext>
{
    public ClinicDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClinicDbContext>();
        // Use a dummy connection string for migration generation
        optionsBuilder.UseNpgsql("Host=localhost;Database=dummy");

        return new ClinicDbContext(optionsBuilder.Options, new DesignTimeTenant());
    }
}

public class DesignTimeTenant : Clinic.Backend.Services.ICurrentTenant
{
    public int? Id => null;
    public string? Name => null;
    public void SetTenant(int id, string name) { }
}
