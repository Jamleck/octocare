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

        // Seed plans and budget categories for the first participant
        await SeedPlansAsync(org.Id, participants[0], providers);

        // Seed price guide version and support items (shared reference data)
        await SeedPriceGuideAsync();

        // Seed default email templates
        await SeedEmailTemplatesAsync(org.Id);

        _logger.LogInformation("Development data seeded successfully");
    }

    private async Task SeedPlansAsync(Guid tenantId, Participant participant, Provider[] providers)
    {
        // Active plan with budget categories
        var activePlan = Plan.Create(tenantId, participant.Id, "NDIS-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        // Activate the plan via reflection since we want it in Active state
        activePlan.Activate();

        _db.Plans.Add(activePlan);
        await _db.SaveChangesAsync();

        // Add budget categories for the active plan
        var categories = new[]
        {
            BudgetCategory.Create(activePlan.Id, SupportCategory.Core,
                SupportPurpose.DailyActivities, 4500000), // $45,000.00
            BudgetCategory.Create(activePlan.Id, SupportCategory.CapacityBuilding,
                SupportPurpose.IncreasedSocialAndCommunityParticipation, 1500000), // $15,000.00
            BudgetCategory.Create(activePlan.Id, SupportCategory.Capital,
                SupportPurpose.AssistiveTechnology, 800000), // $8,000.00
        };

        _db.BudgetCategories.AddRange(categories);
        await _db.SaveChangesAsync();

        // Draft plan (no budget categories yet)
        var draftPlan = Plan.Create(tenantId, participant.Id, "NDIS-2026-001",
            new DateOnly(2026, 7, 1), new DateOnly(2027, 6, 30));

        _db.Plans.Add(draftPlan);
        await _db.SaveChangesAsync();

        // Seed service agreements for the active plan
        await SeedServiceAgreementsAsync(tenantId, participant, activePlan, categories, providers);

        // Seed invoices for the active plan
        var invoice2LineItemIds = await SeedInvoicesAsync(tenantId, participant, activePlan, categories, providers);

        // Seed claims from the approved invoice
        await SeedClaimsAsync(tenantId, invoice2LineItemIds);

        // Seed budget projections for the active plan
        await SeedBudgetProjectionsAsync(categories);
    }

    private async Task SeedServiceAgreementsAsync(Guid tenantId, Participant participant,
        Plan activePlan, BudgetCategory[] categories, Provider[] providers)
    {
        // Agreement 1: Allied Health Plus — 2 items, 1 booking
        var agreement1 = ServiceAgreement.Create(tenantId, participant.Id, providers[0].Id,
            activePlan.Id, new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
        agreement1.Activate();
        _db.ServiceAgreements.Add(agreement1);
        await _db.SaveChangesAsync();

        var item1a = ServiceAgreementItem.Create(agreement1.Id,
            "01_002_0107_1_1", 8445, "weekly"); // $84.45/hr - Assistance with Self-Care Activities
        var item1b = ServiceAgreementItem.Create(agreement1.Id,
            "01_015_0107_1_1", 8214, "fortnightly"); // $82.14/hr - Assistance with Daily Personal Activities
        _db.ServiceAgreementItems.AddRange(item1a, item1b);
        await _db.SaveChangesAsync();

        // Core — Daily Activities booking
        var booking1 = ServiceBooking.Create(agreement1.Id, categories[0].Id, 2000000); // $20,000.00
        _db.ServiceBookings.Add(booking1);
        await _db.SaveChangesAsync();

        // Agreement 2: Therapeutic Solutions — 1 item, 1 booking
        var agreement2 = ServiceAgreement.Create(tenantId, participant.Id, providers[1].Id,
            activePlan.Id, new DateOnly(2025, 7, 1), new DateOnly(2026, 3, 31));
        agreement2.Activate();
        _db.ServiceAgreements.Add(agreement2);
        await _db.SaveChangesAsync();

        var item2 = ServiceAgreementItem.Create(agreement2.Id,
            "04_104_0125_6_1", 9111, "weekly"); // $91.11/hr - Group-Based Community Social and Recreational
        _db.ServiceAgreementItems.Add(item2);
        await _db.SaveChangesAsync();

        // Capacity Building — Social & Community Participation booking
        var booking2 = ServiceBooking.Create(agreement2.Id, categories[1].Id, 800000); // $8,000.00
        _db.ServiceBookings.Add(booking2);
        await _db.SaveChangesAsync();
    }

    private async Task<List<Guid>> SeedInvoicesAsync(Guid tenantId, Participant participant,
        Plan activePlan, BudgetCategory[] categories, Provider[] providers)
    {
        // Invoice 1: Submitted — from Allied Health Plus
        // 4 * 8445 + 4 * 8445 + 8.25 * 8214 = 33780 + 33780 + 67766 = 135326 cents ($1,353.26)
        var invoice1 = Invoice.Create(tenantId, providers[0].Id, participant.Id,
            activePlan.Id, "INV-2025-001",
            new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 31),
            "Weekly assistance with self-care activities");
        SetProperty(invoice1, "TotalAmount", 135326L);

        _db.Invoices.Add(invoice1);
        await _db.SaveChangesAsync();

        _db.InvoiceLineItems.AddRange(
            InvoiceLineItem.Create(invoice1.Id,
                "01_002_0107_1_1", "Assistance with Self-Care Activities - Weekday",
                new DateOnly(2025, 7, 7), 4m, 8445, categories[0].Id),
            InvoiceLineItem.Create(invoice1.Id,
                "01_002_0107_1_1", "Assistance with Self-Care Activities - Weekday",
                new DateOnly(2025, 7, 14), 4m, 8445, categories[0].Id),
            InvoiceLineItem.Create(invoice1.Id,
                "01_015_0107_1_1", "Assistance with Daily Personal Activities",
                new DateOnly(2025, 7, 21), 8.25m, 8214, categories[0].Id)
        );
        await _db.SaveChangesAsync();

        // Invoice 2: Approved — from Therapeutic Solutions
        // 12 * 9111 + 12 * 9111 = 109332 + 109332 = 218664 cents ($2,186.64)
        var invoice2 = Invoice.Create(tenantId, providers[1].Id, participant.Id,
            activePlan.Id, "INV-2025-002",
            new DateOnly(2025, 8, 1), new DateOnly(2025, 8, 31),
            "Group-based community participation activities");
        SetProperty(invoice2, "TotalAmount", 218664L);
        invoice2.Approve();

        _db.Invoices.Add(invoice2);
        await _db.SaveChangesAsync();

        var inv2Line1 = InvoiceLineItem.Create(invoice2.Id,
            "04_104_0125_6_1", "Group-Based Community Activities - Week 1",
            new DateOnly(2025, 8, 4), 12m, 9111, categories[1].Id);
        var inv2Line2 = InvoiceLineItem.Create(invoice2.Id,
            "04_104_0125_6_1", "Group-Based Community Activities - Week 2",
            new DateOnly(2025, 8, 11), 12m, 9111, categories[1].Id);
        _db.InvoiceLineItems.AddRange(inv2Line1, inv2Line2);
        await _db.SaveChangesAsync();

        // Invoice 3: Paid — from Community Care Services
        // 1 * 65000 + 1 * 30000 = 95000 cents ($950.00)
        var invoice3 = Invoice.Create(tenantId, providers[2].Id, participant.Id,
            activePlan.Id, "INV-2025-003",
            new DateOnly(2025, 9, 1), new DateOnly(2025, 9, 30),
            "Assistive equipment delivery");
        SetProperty(invoice3, "TotalAmount", 95000L);
        invoice3.Approve();
        invoice3.MarkPaid();

        _db.Invoices.Add(invoice3);
        await _db.SaveChangesAsync();

        _db.InvoiceLineItems.AddRange(
            InvoiceLineItem.Create(invoice3.Id,
                "05_060_0115_3_1", "Assistive Equipment - Personal Care Item",
                new DateOnly(2025, 9, 10), 1m, 65000, categories[2].Id),
            InvoiceLineItem.Create(invoice3.Id,
                "05_060_0115_3_1", "Assistive Equipment - Delivery & Setup",
                new DateOnly(2025, 9, 15), 1m, 30000, categories[2].Id)
        );
        await _db.SaveChangesAsync();

        return new List<Guid> { inv2Line1.Id, inv2Line2.Id };
    }

    private async Task SeedClaimsAsync(Guid tenantId, List<Guid> approvedLineItemIds)
    {
        // Create a submitted claim from the approved invoice line items
        var claim = Claim.Create(tenantId, "CLM-20250901-SEED01");
        _db.Claims.Add(claim);
        await _db.SaveChangesAsync();

        foreach (var lineItemId in approvedLineItemIds)
        {
            var claimLineItem = ClaimLineItem.Create(claim.Id, lineItemId);
            _db.ClaimLineItems.Add(claimLineItem);
        }
        await _db.SaveChangesAsync();

        // Set the total amount and submit the claim
        SetProperty(claim, "TotalAmount", 218664L); // matches invoice2 total
        claim.Submit();
        await _db.SaveChangesAsync();
    }

    private async Task SeedBudgetProjectionsAsync(BudgetCategory[] categories)
    {
        // Core — Daily Activities: Allocated $45,000; Committed $20,000 (active booking);
        // Spent $0 (no approved/paid invoices for this category); Pending $1,353.26 (submitted invoice1)
        var coreProjection = BudgetProjection.Create(categories[0].Id, categories[0].AllocatedAmount);
        coreProjection.UpdateFromEvent(
            allocatedCents: 4500000,
            committedCents: 2000000,
            spentCents: 0,
            pendingCents: 135326);

        // Capacity Building — Social & Community: Allocated $15,000; Committed $8,000 (active booking);
        // Spent $2,186.64 (approved invoice2); Pending $0
        var cbProjection = BudgetProjection.Create(categories[1].Id, categories[1].AllocatedAmount);
        cbProjection.UpdateFromEvent(
            allocatedCents: 1500000,
            committedCents: 800000,
            spentCents: 218664,
            pendingCents: 0);

        // Capital — Assistive Technology: Allocated $8,000; Committed $0;
        // Spent $950 (paid invoice3); Pending $0
        var capitalProjection = BudgetProjection.Create(categories[2].Id, categories[2].AllocatedAmount);
        capitalProjection.UpdateFromEvent(
            allocatedCents: 800000,
            committedCents: 0,
            spentCents: 95000,
            pendingCents: 0);

        _db.BudgetProjections.AddRange(coreProjection, cbProjection, capitalProjection);
        await _db.SaveChangesAsync();
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

    private async Task SeedEmailTemplatesAsync(Guid tenantId)
    {
        var templates = new (string Name, string Subject, string Body)[]
        {
            ("invoice_submitted",
             "Invoice {{invoice_number}} Submitted",
             "<h2>Invoice Submitted</h2><p>Invoice <strong>{{invoice_number}}</strong> has been submitted by {{provider_name}} for participant {{participant_name}}.</p><p>Amount: {{amount}}</p><p>Please review and approve the invoice at your earliest convenience.</p>"),

            ("plan_expiring",
             "Plan {{plan_number}} Expiring Soon",
             "<h2>Plan Expiring</h2><p>Plan <strong>{{plan_number}}</strong> for {{participant_name}} will expire on {{expiry_date}}.</p><p>Days remaining: {{days_remaining}}</p><p>Please begin the plan transition process to ensure continuity of services.</p>"),

            ("budget_alert",
             "Budget Alert: {{category_name}}",
             "<h2>Budget Alert</h2><p>A budget alert has been generated for plan {{plan_number}}.</p><p>Category: {{category_name}}</p><p>Utilisation: {{utilisation}}%</p><p>{{alert_message}}</p>"),

            ("statement_ready",
             "Participant Statement Ready - {{participant_name}}",
             "<h2>Statement Ready</h2><p>A new participant statement has been generated for <strong>{{participant_name}}</strong>.</p><p>Period: {{period_start}} to {{period_end}}</p><p>The statement is now available for review and distribution.</p>")
        };

        foreach (var (name, subject, body) in templates)
        {
            var template = Domain.Entities.EmailTemplate.Create(tenantId, name, subject, body);
            _db.EmailTemplates.Add(template);
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} default email templates", templates.Length);
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
