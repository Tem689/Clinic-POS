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

public class AppointmentTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AppointmentTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Messaging:UseInMemory"] = "true",
                    ["Jwt:Key"] = "super_secret_key_for_testing_purposes_only_12345",
                    ["Jwt:Issuer"] = "clinic_pos",
                    ["Jwt:Audience"] = "clinic_pos_users"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IDistributedCache>(new Mock<IDistributedCache>().Object);
                services.AddSingleton<IPublishEndpoint>(new Mock<IPublishEndpoint>().Object);
                services.AddSingleton<IBus>(new Mock<IBus>().Object);
            });
        });
    }

    [Fact]
    public async Task CreateDuplicateAppointment_ShouldFail()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Clinic A" };
        var branch = new Branch { Tenant = tenant, Name = "Branch 1" };
        var patient = new Patient { Tenant = tenant, FirstName = "John", LastName = "Doe", PhoneNumber = "123" };
        db.Tenants.Add(tenant);
        db.Branches.Add(branch);
        db.Patients.Add(patient);
        await db.SaveChangesAsync();

        var client = CreateAuthenticatedClient(tenant.Id, "Admin");
        var startTime = DateTime.UtcNow.AddHours(1);

        // 1. First booking
        await client.PostAsJsonAsync("/api/appointments", new { patientId = patient.Id, branchId = branch.Id, startAt = startTime });

        // 2. Second booking (Same patient, same time, same branch) -> Should Conflict
        var response = await client.PostAsJsonAsync("/api/appointments", new { patientId = patient.Id, branchId = branch.Id, startAt = startTime });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
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
