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

public class AuthTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public AuthTests(WebApplicationFactory<Program> factory)
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
    public async Task Viewer_Cannot_CreatePatient()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Test Tenant" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var client = CreateAuthenticatedClient(tenant.Id, "Viewer");
        
        var response = await client.PostAsJsonAsync("/api/patients", new { 
            firstName = "Illegal", 
            lastName = "Entry", 
            phoneNumber = "000" 
        });

        // Requirement B2: Viewer cannot create patients
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task NonAdmin_Cannot_CreateUser()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Test Tenant" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var client = CreateAuthenticatedClient(tenant.Id, "User");
        
        var response = await client.PostAsJsonAsync("/api/users", new { 
            email = "new@test.com", 
            role = "User", 
            tenantId = tenant.Id 
        });

        // Only Admin can access UsersController
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Admin_Can_CreateUser()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var tenant = new Tenant { Name = "Test Tenant" };
        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var client = CreateAuthenticatedClient(tenant.Id, "Admin");
        
        var response = await client.PostAsJsonAsync("/api/users", new { 
            email = "newadmin@test.com", 
            role = "User", 
            tenantId = tenant.Id 
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
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
