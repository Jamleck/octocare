using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Infrastructure.Data.Seeding;

public class DevDataSeeder
{
    private readonly OctocareDbContext _db;
    private readonly ILogger<DevDataSeeder> _logger;

    // Well-known IDs for development
    public static readonly Guid DevOrgId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
    public static readonly Guid DevAdminUserId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901");

    public DevDataSeeder(OctocareDbContext db, ILogger<DevDataSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Skip if already seeded
        if (await _db.Organisations.IgnoreQueryFilters().AnyAsync(o => o.Id == DevOrgId))
        {
            _logger.LogDebug("Dev data already seeded, skipping");
            return;
        }

        _logger.LogInformation("Seeding development data...");

        var org = Organisation.Create(
            "Acme Plan Management",
            "51824753556",
            "admin@acmepm.com.au",
            "0400000000",
            "123 Main St, Sydney NSW 2000");
        // Set well-known IDs via reflection for dev seeding
        SetId(org, DevOrgId);
        SetProperty(org, nameof(Organisation.TenantId), DevOrgId);
        _db.Organisations.Add(org);

        var admin = User.Create("auth0|dev-admin", "admin@acmepm.com.au", "Admin", "User");
        SetId(admin, DevAdminUserId);
        _db.Users.Add(admin);

        var pmUser = User.Create("auth0|dev-pm", "pm@acmepm.com.au", "Jane", "Smith");
        _db.Users.Add(pmUser);

        var financeUser = User.Create("auth0|dev-finance", "finance@acmepm.com.au", "Bob", "Jones");
        _db.Users.Add(financeUser);

        await _db.SaveChangesAsync();

        // Add memberships (need org saved first for FK)
        _db.UserOrgMemberships.Add(UserOrgMembership.Create(admin.Id, org.Id, OrgRole.OrgAdmin));
        _db.UserOrgMemberships.Add(UserOrgMembership.Create(pmUser.Id, org.Id, OrgRole.PlanManager));
        _db.UserOrgMemberships.Add(UserOrgMembership.Create(financeUser.Id, org.Id, OrgRole.Finance));

        // Seed participants
        var participants = new[]
        {
            Participant.Create(org.Id, "431234567", "Sarah", "Johnson",
                new DateOnly(1985, 3, 15), "sarah.j@email.com", "0412345678",
                "45 Oak Ave, Melbourne VIC 3000",
                "Margaret Johnson", "margaret.j@email.com", "0498765432", "Mother"),
            Participant.Create(org.Id, "432345678", "Michael", "Chen",
                new DateOnly(1992, 7, 22), "m.chen@email.com", "0423456789",
                "12 Pine Rd, Brisbane QLD 4000"),
            Participant.Create(org.Id, "433456789", "Emily", "Williams",
                new DateOnly(1978, 11, 3), "emily.w@email.com", "0434567890",
                "78 Elm St, Perth WA 6000",
                "David Williams", "david.w@email.com", "0445678901", "Spouse"),
            Participant.Create(org.Id, "434567890", "James", "Brown",
                new DateOnly(2001, 1, 30), "james.b@email.com", "0445678901",
                "3 Cedar Ln, Adelaide SA 5000"),
            Participant.Create(org.Id, "435678901", "Olivia", "Taylor",
                new DateOnly(1995, 9, 8), "olivia.t@email.com", "0456789012",
                "91 Birch Dr, Hobart TAS 7000",
                "Robert Taylor", "robert.t@email.com", "0467890123", "Father"),
        };

        _db.Participants.AddRange(participants);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Development data seeded successfully");
    }

    private static void SetId<T>(T entity, Guid id)
    {
        typeof(T).GetProperty("Id")!.SetValue(entity, id);
    }

    private static void SetProperty<T>(T entity, string name, object value)
    {
        typeof(T).GetProperty(name)!.SetValue(entity, value);
    }
}
