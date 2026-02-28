using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public class PmFeeService
{
    public const string PmMonthlyFeeItemNumber = "15_037_0117_1_3";
    public const string PmSetupFeeItemNumber = "15_038_0117_1_3";
    public const string PmFeeSource = "pm_fee_auto";

    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IPlanRepository _planRepo;
    private readonly IPriceGuideRepository _priceGuideRepo;
    private readonly IProviderRepository _providerRepo;
    private readonly ITenantContext _tenantContext;
    private readonly IEventStore _eventStore;

    public PmFeeService(
        IInvoiceRepository invoiceRepo,
        IParticipantRepository participantRepo,
        IPlanRepository planRepo,
        IPriceGuideRepository priceGuideRepo,
        IProviderRepository providerRepo,
        ITenantContext tenantContext,
        IEventStore eventStore)
    {
        _invoiceRepo = invoiceRepo;
        _participantRepo = participantRepo;
        _planRepo = planRepo;
        _priceGuideRepo = priceGuideRepo;
        _providerRepo = providerRepo;
        _tenantContext = tenantContext;
        _eventStore = eventStore;
    }

    /// <summary>
    /// Generates PM fee invoices for all active participants with active plans for the given month/year.
    /// Creates one invoice per participant with a single line item for the PM monthly fee.
    /// </summary>
    public async Task<GenerateMonthlyFeesResponse> GenerateMonthlyFeesAsync(int month, int year, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        // Get the current price guide and look up the PM monthly fee rate
        var currentVersion = await _priceGuideRepo.GetCurrentVersionAsync(ct)
            ?? throw new InvalidOperationException("No current price guide version found.");

        var supportItem = await _priceGuideRepo.GetItemByNumberAsync(currentVersion.Id, PmMonthlyFeeItemNumber, ct)
            ?? throw new InvalidOperationException($"PM monthly fee support item '{PmMonthlyFeeItemNumber}' not found in current price guide.");

        var rateCents = supportItem.PriceLimitNational;

        // Get all active participants
        var participants = await _participantRepo.GetAllActiveAsync(ct);

        var invoiceIds = new List<Guid>();

        foreach (var participant in participants)
        {
            // Find the active plan for this participant
            var plan = await _planRepo.GetActivePlanForParticipantAsync(participant.Id, ct);
            if (plan is null)
                continue; // Skip participants without active plans

            // Check if the month falls within the plan period
            var servicePeriodStart = new DateOnly(year, month, 1);
            var servicePeriodEnd = servicePeriodStart.AddMonths(1).AddDays(-1);

            if (servicePeriodStart > plan.EndDate || servicePeriodEnd < plan.StartDate)
                continue; // Skip if the month is outside the plan period

            // Find the first linked provider to use as the "self" provider for PM fees.
            // In a real system, this would be the tenant's own provider record.
            var providerId = await GetFirstLinkedProviderIdAsync(tenantId, ct);

            // Generate a unique invoice number
            var invoiceNumber = $"PMFEE-{year:D4}{month:D2}-{participant.NdisNumber}";

            // Check for duplicate (idempotency)
            if (await _invoiceRepo.ExistsByInvoiceNumberAsync(invoiceNumber, ct: ct))
                continue;

            // Find the Improved Daily Living Skills budget category for this plan
            var budgetCategoryId = plan.BudgetCategories
                .FirstOrDefault(bc => bc.SupportPurpose == Domain.Enums.SupportPurpose.ImprovedDailyLivingSkills)?.Id;

            // Create the invoice
            var invoice = Invoice.Create(
                tenantId,
                providerId,
                participant.Id,
                plan.Id,
                invoiceNumber,
                servicePeriodStart,
                servicePeriodEnd,
                $"Plan Management Monthly Fee - {servicePeriodStart:MMMM yyyy}");
            invoice.SetSource(PmFeeSource);

            // Add a single line item for the PM monthly fee
            var lineItem = InvoiceLineItem.Create(
                invoice.Id,
                PmMonthlyFeeItemNumber,
                supportItem.Name,
                servicePeriodStart, // service date is first day of month
                1m, // quantity: 1
                rateCents,
                budgetCategoryId);

            invoice.LineItems.Add(lineItem);
            invoice.RecalculateTotal();

            await _invoiceRepo.AddAsync(invoice, ct);

            // Append event
            await _eventStore.AppendAsync(
                invoice.Id,
                "Invoice",
                "PmFeeGenerated",
                new
                {
                    invoice.InvoiceNumber,
                    invoice.ParticipantId,
                    invoice.PlanId,
                    FeeType = "monthly",
                    TotalAmountCents = invoice.TotalAmount,
                    Month = month,
                    Year = year
                },
                0,
                null,
                ct);

            invoiceIds.Add(invoice.Id);
        }

        return new GenerateMonthlyFeesResponse(invoiceIds.Count, invoiceIds);
    }

    /// <summary>
    /// Generates a one-time PM setup fee invoice for the specified participant.
    /// </summary>
    public async Task<GenerateSetupFeeResponse> GenerateSetupFeeAsync(Guid participantId, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        // Get the current price guide and look up the PM setup fee rate
        var currentVersion = await _priceGuideRepo.GetCurrentVersionAsync(ct)
            ?? throw new InvalidOperationException("No current price guide version found.");

        var supportItem = await _priceGuideRepo.GetItemByNumberAsync(currentVersion.Id, PmSetupFeeItemNumber, ct)
            ?? throw new InvalidOperationException($"PM setup fee support item '{PmSetupFeeItemNumber}' not found in current price guide.");

        var rateCents = supportItem.PriceLimitNational;

        // Verify participant exists
        var participant = await _participantRepo.GetByIdAsync(participantId, ct)
            ?? throw new KeyNotFoundException($"Participant '{participantId}' not found.");

        // Find the active plan
        var plan = await _planRepo.GetActivePlanForParticipantAsync(participantId, ct)
            ?? throw new InvalidOperationException($"No active plan found for participant '{participantId}'.");

        // Find provider for self-billing
        var providerId = await GetFirstLinkedProviderIdAsync(tenantId, ct);

        // Generate a unique invoice number
        var invoiceNumber = $"PMSETUP-{participant.NdisNumber}-{plan.PlanNumber}";

        // Check for duplicate (idempotency)
        if (await _invoiceRepo.ExistsByInvoiceNumberAsync(invoiceNumber, ct: ct))
            throw new InvalidOperationException($"A setup fee invoice already exists for participant '{participantId}' on plan '{plan.PlanNumber}'.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Find the Improved Daily Living Skills budget category
        var budgetCategoryId = plan.BudgetCategories
            .FirstOrDefault(bc => bc.SupportPurpose == Domain.Enums.SupportPurpose.ImprovedDailyLivingSkills)?.Id;

        // Create the invoice
        var invoice = Invoice.Create(
            tenantId,
            providerId,
            participantId,
            plan.Id,
            invoiceNumber,
            today,
            today,
            $"Plan Management Setup Fee - {participant.FullName}");
        invoice.SetSource(PmFeeSource);

        // Add a single line item for the PM setup fee
        var lineItem = InvoiceLineItem.Create(
            invoice.Id,
            PmSetupFeeItemNumber,
            supportItem.Name,
            today,
            1m,
            rateCents,
            budgetCategoryId);

        invoice.LineItems.Add(lineItem);
        invoice.RecalculateTotal();

        await _invoiceRepo.AddAsync(invoice, ct);

        // Append event
        await _eventStore.AppendAsync(
            invoice.Id,
            "Invoice",
            "PmFeeGenerated",
            new
            {
                invoice.InvoiceNumber,
                invoice.ParticipantId,
                invoice.PlanId,
                FeeType = "setup",
                TotalAmountCents = invoice.TotalAmount
            },
            0,
            null,
            ct);

        return new GenerateSetupFeeResponse(invoice.Id);
    }

    /// <summary>
    /// Gets the first provider linked to the tenant. Used as the "self" provider for PM fee invoices.
    /// In a production system, this would be the tenant's own provider record.
    /// </summary>
    private async Task<Guid> GetFirstLinkedProviderIdAsync(Guid tenantId, CancellationToken ct)
    {
        // Get the first provider from the paged list (all returned providers are linked to this tenant)
        var (providers, _) = await _providerRepo.GetPagedAsync(1, 1, ct: ct);

        if (providers.Count == 0)
            throw new InvalidOperationException("No providers linked to the current tenant. At least one provider is required for PM fee generation.");

        return providers[0].Id;
    }
}
