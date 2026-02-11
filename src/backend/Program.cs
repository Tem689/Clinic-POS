using System.Text;
using Clinic.Backend.Data;
using Clinic.Backend.Middleware;
using Clinic.Backend.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Configuration
if (builder.Environment.EnvironmentName == "Testing")
{
    builder.Services.AddDbContext<ClinicDbContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
}
else
{
    builder.Services.AddDbContext<ClinicDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// 2. Tenancy
builder.Services.AddScoped<ICurrentTenant, CurrentTenant>();

// 3. Auth & Identity
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "super_secret_key_for_testing_purposes_only_12345"))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanViewPatients", policy => policy.RequireRole("Admin", "User", "Viewer"));
    options.AddPolicy("CanCreatePatients", policy => policy.RequireRole("Admin", "User"));
    options.AddPolicy("CanCreateAppointments", policy => policy.RequireRole("Admin", "User"));
});

// 4. Caching (Redis)
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Clinic_";
});

// 5. Messaging (RabbitMQ or In-Memory for tests)
builder.Services.AddMassTransit(x =>
{
    if (builder.Configuration["Messaging:UseInMemory"] == "true")
    {
        x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    }
    else
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(builder.Configuration["RabbitMq:Host"] ?? "localhost", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });
        });
    }
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 6. Database Migrations & Seeding
if (app.Environment.EnvironmentName != "Testing")
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ClinicDbContext>();
        await context.Database.MigrateAsync();
        await DataSeeder.SeedAsync(context);
    }
}

// HTTPS redirection disabled for Docker thin slice stability
// if (!app.Environment.IsDevelopment())
// {
//     app.UseHttpsRedirection();
// }

app.UseCors("AllowFrontend");

app.UseAuthentication();
// Custom Tenant Middleware must run after Authentication to have access to User claims
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
