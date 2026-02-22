using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Octocare.Infrastructure.Data.Seeding;

namespace Octocare.Api.Authentication;

public class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevAuth";

    private static readonly Dictionary<string, (string Sub, string Email, string Name)> DevUsers = new()
    {
        ["admin"] = ("auth0|dev-admin", "admin@acmepm.com.au", "Admin User"),
        ["pm"] = ("auth0|dev-pm", "pm@acmepm.com.au", "Jane Smith"),
        ["finance"] = ("auth0|dev-finance", "finance@acmepm.com.au", "Bob Jones"),
    };

    public DevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogWarning("DevAuthHandler is active — all requests are auto-authenticated. DO NOT use in production.");

        var userKey = Context.Request.Headers["X-Dev-User"].FirstOrDefault()?.ToLowerInvariant() ?? "admin";

        if (!DevUsers.TryGetValue(userKey, out var devUser))
            devUser = DevUsers["admin"];

        var claims = new[]
        {
            new Claim("sub", devUser.Sub),
            new Claim("email", devUser.Email),
            new Claim("name", devUser.Name),
            new Claim("org_id", DevDataSeeder.DevOrgId.ToString()),
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
