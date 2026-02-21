using Octocare.Api.Middleware;
using Octocare.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Infrastructure (DbContext, TenantContext, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Auth:Authority"];
        options.Audience = builder.Configuration["Auth:Audience"];
    });
builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173"];
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Development-only middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantResolutionMiddleware>();
app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();
