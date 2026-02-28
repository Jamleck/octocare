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
    public static readonly Guid DevPriceGuideVersionId = Guid.Parse("c3d4e5f6-a7b8-9012-cdef-234567890123");

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

        // Seed providers (shared across tenants)
        var providers = new[]
        {
            Provider.Create("Allied Health Plus", "53004085616",
                "contact@alliedhealthplus.com.au", "0290001111",
                "10 George St, Sydney NSW 2000"),
            Provider.Create("Therapeutic Solutions", "72004085616",
                "info@therapeuticsolutions.com.au", "0390002222",
                "25 Collins St, Melbourne VIC 3000"),
            Provider.Create("Community Care Services", "85004085616",
                "hello@communitycare.com.au", "0790003333",
                "8 Adelaide Tce, Perth WA 6000"),
        };

        _db.Providers.AddRange(providers);
        await _db.SaveChangesAsync();

        // Link providers to the dev org via TenantProviderRelationships
        foreach (var provider in providers)
        {
            _db.TenantProviderRelationships.Add(
                TenantProviderRelationship.Create(org.Id, provider.Id));
        }
        await _db.SaveChangesAsync();

        // Seed price guide version and support items (shared reference data)
        await SeedPriceGuideAsync();

        _logger.LogInformation("Development data seeded successfully");
    }

    private async Task SeedPriceGuideAsync()
    {
        var version = PriceGuideVersion.Create(
            "2025-26",
            new DateOnly(2025, 7, 1),
            new DateOnly(2026, 6, 30));
        SetId(version, DevPriceGuideVersionId);
        version.SetCurrent(true);

        _db.PriceGuideVersions.Add(version);
        await _db.SaveChangesAsync();

        var items = new[]
        {
            // Core — Daily Activities
            SupportItem.Create(version.Id,
                "01_011_0107_1_1", "Assistance with Daily Life Activities in a Group",
                SupportCategory.Core, SupportPurpose.DailyActivities,
                UnitOfMeasure.Hour, 6547, 8184, 9820,
                true, CancellationRule.ShortNotice2Day, ClaimType.Time),

            SupportItem.Create(version.Id,
                "01_002_0107_1_1", "Assistance with Self-Care Activities",
                SupportCategory.Core, SupportPurpose.DailyActivities,
                UnitOfMeasure.Hour, 6756, 8445, 10134,
                true, CancellationRule.ShortNotice2Day, ClaimType.Time),

            SupportItem.Create(version.Id,
                "01_015_0107_1_1", "Assistance with Daily Personal Activities - Standard",
                SupportCategory.Core, SupportPurpose.DailyActivities,
                UnitOfMeasure.Hour, 6571, 8214, 9857,
                true, CancellationRule.ShortNotice2Day, ClaimType.Time),

            SupportItem.Create(version.Id,
                "01_019_0107_1_1", "Assistance with Daily Personal Activities - Night-Time",
                SupportCategory.Core, SupportPurpose.DailyActivities,
                UnitOfMeasure.Hour, 7238, 9048, 10857,
                true, CancellationRule.ShortNotice2Day, ClaimType.Time),

            // Core — Transport
            SupportItem.Create(version.Id,
                "02_051_0108_1_1", "Transport - Provider Travel",
                SupportCategory.Core, SupportPurpose.TransportActivities,
                UnitOfMeasure.Each, 5000, 6250, 7500,
                false, CancellationRule.None, ClaimType.NonTime),

            // Capacity Building — Social & Community Participation
            SupportItem.Create(version.Id,
                "04_104_0125_6_1", "Group-Based Community Social and Recreational Activities",
                SupportCategory.CapacityBuilding, SupportPurpose.IncreasedSocialAndCommunityParticipation,
                UnitOfMeasure.Hour, 7289, 9111, 10934,
                true, CancellationRule.ShortNotice2Day, ClaimType.Time),

            SupportItem.Create(version.Id,
                "04_105_0125_6_1", "Individual Social and Community Participation",
                SupportCategory.CapacityBuilding, SupportPurpose.IncreasedSocialAndCommunityParticipation,
                UnitOfMeasure.Hour, 6571, 8214, 9857,
                true, CancellationRule.ShortNotice2Day, ClaimType.Time),

            // Capacity Building — Finding & Keeping a Job
            SupportItem.Create(version.Id,
                "10_020_0120_5_1", "Employment Related Assessment and Counselling",
                SupportCategory.CapacityBuilding, SupportPurpose.FindingAndKeepingAJob,
                UnitOfMeasure.Hour, 6571, 8214, 9857,
                false, CancellationRule.ShortNotice2Day, ClaimType.Time),

            // Capacity Building — Improved Daily Living Skills
            SupportItem.Create(version.Id,
                "15_037_0117_1_3", "Plan Management Monthly Fee",
                SupportCategory.CapacityBuilding, SupportPurpose.ImprovedDailyLivingSkills,
                UnitOfMeasure.Each, 3628, 3628, 3628,
                false, CancellationRule.None, ClaimType.NonTime),

            SupportItem.Create(version.Id,
                "15_038_0117_1_3", "Plan Management Setup Fee",
                SupportCategory.CapacityBuilding, SupportPurpose.ImprovedDailyLivingSkills,
                UnitOfMeasure.Each, 3628, 3628, 3628,
                false, CancellationRule.None, ClaimType.NonTime),

            SupportItem.Create(version.Id,
                "15_040_0117_1_3", "Plan Management Claim Fee",
                SupportCategory.CapacityBuilding, SupportPurpose.ImprovedDailyLivingSkills,
                UnitOfMeasure.Each, 1093, 1093, 1093,
                false, CancellationRule.None, ClaimType.NonTime),

            // Capacity Building — Coordination of Supports
            SupportItem.Create(version.Id,
                "07_001_0106_6_3", "Coordination of Supports - Level 1",
                SupportCategory.CapacityBuilding, SupportPurpose.CoordinationOfSupports,
                UnitOfMeasure.Hour, 6571, 8214, 9857,
                false, CancellationRule.None, ClaimType.Time),

            // Capital — Assistive Technology
            SupportItem.Create(version.Id,
                "05_060_0115_3_1", "Assistive Equipment - Personal Care",
                SupportCategory.Capital, SupportPurpose.AssistiveTechnology,
                UnitOfMeasure.Each, 150000, 150000, 150000,
                false, CancellationRule.None, ClaimType.NonTime),

            // Capital — Home Modifications
            SupportItem.Create(version.Id,
                "06_070_0116_4_1", "Home Modifications",
                SupportCategory.Capital, SupportPurpose.HomeModifications,
                UnitOfMeasure.Each, 2000000, 2000000, 2000000,
                false, CancellationRule.None, ClaimType.NonTime),
        };

        _db.SupportItems.AddRange(items);
        await _db.SaveChangesAsync();
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
