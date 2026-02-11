using System.Net;
using System.Net.Http.Json;
using Clinic.Backend.Data;
using Clinic.Backend.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using MassTransit;
using Moq;
using Xunit;

namespace Clinic.Tests;

public class PatientTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PatientTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Redis"] = "localhost:6379,abortConnect=false",
                    ["Messaging:UseInMemory"] = "true",
                    ["Jwt:Key"] = "super_secret_key_for_testing_purposes_only_12345",
                    ["Jwt:Issuer"] = "clinic_pos",
                    ["Jwt:Audience"] = "clinic_pos_users"
                });
            });

            builder.ConfigureServices(services =>
            {
                // Mock Redis
                services.AddSingleton<IDistributedCache>(new Mock<IDistributedCache>().Object);

                // Mock MassTransit (to prevent RabbitMQ connection attempts)
                services.AddSingleton<IPublishEndpoint>(new Mock<IPublishEndpoint>().Object);
                services.AddSingleton<IBus>(new Mock<IBus>().Object);
            });
        });
    }

    [Fact]
    public async Task CreatePatient_ShouldEnforceTenantIsolation()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenantA = new Tenant { Name = "Tenant A" };
        var tenantB = new Tenant { Name = "Tenant B" };
        db.Tenants.AddRange(tenantA, tenantB);
        await db.SaveChangesAsync();

        var clientA = CreateAuthenticatedClient(tenantA.Id, "Admin");
        await clientA.PostAsJsonAsync("/api/patients", new { firstName = "Alice", lastName = "A", phoneNumber = "111", primaryBranchId = (int?)null });

        var clientB = CreateAuthenticatedClient(tenantB.Id, "Admin");
        var response = await clientB.GetFromJsonAsync<List<Patient>>("/api/patients");
        
        Assert.DoesNotContain(response!, p => p.FirstName == "Alice");
    }

    [Fact]
    public async Task CreateDuplicatePhone_InSameTenant_ShouldFail()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Solo Tenant" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var client = CreateAuthenticatedClient(tenant.Id, "Admin");
        await client.PostAsJsonAsync("/api/patients", new { firstName = "Bob", lastName = "1", phoneNumber = "555", primaryBranchId = (int?)null });

        var response = await client.PostAsJsonAsync("/api/patients", new { firstName = "Bob", lastName = "2", phoneNumber = "555", primaryBranchId = (int?)null });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreatePatient_ShouldInvalidateCache()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Cache Test Tenant" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var client = CreateAuthenticatedClient(tenant.Id, "Admin");

        // 1. First request to populate cache
        await client.GetFromJsonAsync<List<Patient>>("/api/patients");

        // 2. Create a patient (should trigger invalidation)
        await client.PostAsJsonAsync("/api/patients", new { firstName = "New", lastName = "Patient", phoneNumber = "999" });

        // 3. Verify patient appears in subsequent list request
        var response = await client.GetFromJsonAsync<List<Patient>>("/api/patients");
        Assert.Contains(response!, p => p.FirstName == "New");
    }

    private HttpClient CreateAuthenticatedClient(int tenantId, string role)
    {
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var tokenService = scope.ServiceProvider.GetRequiredService<Clinic.Backend.Services.ITokenService>();
        var token = tokenService.GenerateToken(1, tenantId, "test@test.com", role);
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
