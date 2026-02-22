using Octocare.Api.Authorization;
using Octocare.Api.Middleware;
using Octocare.Application;
using Octocare.Application.Interfaces;
using Octocare.Infrastructure;
using Octocare.Infrastructure.Auth;
using Octocare.Infrastructure.Data;
using Octocare.Infrastructure.Data.Seeding;
using Octocare.Api.Authentication;
using Octocare.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Aspire service defaults (OpenTelemetry, health checks, service discovery, resilience)
builder.AddServiceDefaults();

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Application services
builder.Services.AddApplication();

// Infrastructure (DbContext via Aspire, TenantContext, repositories, etc.)
builder.AddInfrastructure();

// Authentication
if (builder.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Auth:DevBypass"))
{
    builder.Services.AddAuthentication(DevAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevAuthHandler>(DevAuthHandler.SchemeName, _ => { });
}
else
{
    builder.Services.AddAuthentication("Bearer")
        .AddJwtBearer(options =>
        {
            options.Authority = builder.Configuration["Auth:Authority"];
            options.Audience = builder.Configuration["Auth:Audience"];
        });
}

// Authorization (RBAC policies)
builder.Services.AddOctocareAuthorization();

// Current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin =>
                    new Uri(origin).Host == "localhost")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
        else
        {
            var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? [];
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        }
    });
});

var app = builder.Build();

// Aspire default endpoints (/health, /alive)
app.MapDefaultEndpoints();

// Development-only middleware & seeding
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    // Auto-migrate and seed
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OctocareDbContext>();
    await db.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DevDataSeeder>();
    await seeder.SeedAsync();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseMiddleware<TenantResolutionMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
