using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class ReportService
{
    private readonly IParticipantRepository _participantRepo;
    private readonly IPlanRepository _planRepo;
    private readonly IBudgetProjectionRepository _projectionRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IClaimRepository _claimRepo;
    private readonly IEventStore _eventStore;
    private readonly BudgetProjectionService _budgetProjectionService;

    public ReportService(
        IParticipantRepository participantRepo,
        IPlanRepository planRepo,
        IBudgetProjectionRepository projectionRepo,
        IInvoiceRepository invoiceRepo,
        IClaimRepository claimRepo,
        IEventStore eventStore,
        BudgetProjectionService budgetProjectionService)
    {
        _participantRepo = participantRepo;
        _planRepo = planRepo;
        _projectionRepo = projectionRepo;
        _invoiceRepo = invoiceRepo;
        _claimRepo = claimRepo;
        _eventStore = eventStore;
        _budgetProjectionService = budgetProjectionService;
    }

    public async Task<IReadOnlyList<BudgetUtilisationReportRow>> GetBudgetUtilisationAsync(CancellationToken ct)
    {
        var rows = new List<BudgetUtilisationReportRow>();

        var participants = await _participantRepo.GetAllActiveAsync(ct);

        foreach (var participant in participants)
        {
            var plans = await _planRepo.GetByParticipantIdAsync(participant.Id, ct);
            var activePlans = plans.Where(p => p.Status == PlanStatus.Active || p.Status == PlanStatus.Expiring);

            foreach (var plan in activePlans)
            {
                var overview = await _budgetProjectionService.GetProjectionsForPlanAsync(plan.Id, ct);
                if (overview is null) continue;

                foreach (var cat in overview.Categories)
                {
                    rows.Add(new BudgetUtilisationReportRow(
                        ParticipantName: participant.FullName,
                        NdisNumber: participant.NdisNumber,
                        PlanNumber: plan.PlanNumber,
                        Category: cat.SupportCategory,
                        Purpose: cat.SupportPurpose,
                        Allocated: cat.Allocated,
                        Spent: cat.Spent,
                        Available: cat.Available,
                        UtilisationPercent: cat.UtilisationPercentage));
                }
            }
        }

        return rows;
    }

    public async Task<IReadOnlyList<OutstandingInvoiceRow>> GetOutstandingInvoicesAsync(CancellationToken ct)
    {
        var rows = new List<OutstandingInvoiceRow>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Fetch invoices that are not paid and not rejected
        var statuses = new[] { InvoiceStatus.Submitted, InvoiceStatus.UnderReview, InvoiceStatus.Approved, InvoiceStatus.Disputed };

        foreach (var status in statuses)
        {
            var page = 1;
            while (true)
            {
                var (items, totalCount) = await _invoiceRepo.GetPagedAsync(page, 100, status, ct: ct);
                foreach (var invoice in items)
                {
                    var daysOutstanding = today.DayNumber - invoice.ServicePeriodEnd.DayNumber;
                    if (daysOutstanding < 0) daysOutstanding = 0;

                    var ageBucket = daysOutstanding switch
                    {
                        <= 30 => "0-30 days",
                        <= 60 => "31-60 days",
                        <= 90 => "61-90 days",
                        _ => "90+ days"
                    };

                    rows.Add(new OutstandingInvoiceRow(
                        InvoiceNumber: invoice.InvoiceNumber,
                        ProviderName: invoice.Provider?.Name ?? "Unknown",
                        ParticipantName: invoice.Participant?.FullName ?? "Unknown",
                        ServicePeriodEnd: invoice.ServicePeriodEnd,
                        Amount: new Money(invoice.TotalAmount).ToDollars(),
                        Status: invoice.Status,
                        DaysOutstanding: daysOutstanding,
                        AgeBucket: ageBucket));
                }

                if (page * 100 >= totalCount) break;
                page++;
            }
        }

        return rows.OrderByDescending(r => r.DaysOutstanding).ToList();
    }

    public async Task<IReadOnlyList<ClaimStatusRow>> GetClaimStatusAsync(CancellationToken ct)
    {
        var rows = new List<ClaimStatusRow>();
        var page = 1;

        while (true)
        {
            var (items, totalCount) = await _claimRepo.GetPagedAsync(page, 100, ct: ct);
            foreach (var claim in items)
            {
                var acceptedCount = claim.LineItems.Count(li => li.Status == ClaimLineItemStatus.Accepted);
                var rejectedCount = claim.LineItems.Count(li => li.Status == ClaimLineItemStatus.Rejected);

                rows.Add(new ClaimStatusRow(
                    BatchNumber: claim.BatchNumber,
                    Status: claim.Status,
                    TotalAmount: new Money(claim.TotalAmount).ToDollars(),
                    LineItemCount: claim.LineItems.Count,
                    AcceptedCount: acceptedCount,
                    RejectedCount: rejectedCount,
                    SubmissionDate: claim.SubmissionDate));
            }

            if (page * 100 >= totalCount) break;
            page++;
        }

        return rows;
    }

    public async Task<IReadOnlyList<ParticipantSummaryRow>> GetParticipantSummaryAsync(CancellationToken ct)
    {
        var rows = new List<ParticipantSummaryRow>();

        // Get all participants (not just active) for comprehensive summary
        var (participants, _) = await _participantRepo.GetPagedAsync(1, 10000, ct: ct);

        foreach (var participant in participants)
        {
            var plans = await _planRepo.GetByParticipantIdAsync(participant.Id, ct);
            var activePlan = plans.FirstOrDefault(p => p.Status == PlanStatus.Active || p.Status == PlanStatus.Expiring);

            decimal totalAllocated = 0m;
            decimal totalSpent = 0m;

            if (activePlan is not null)
            {
                var overview = await _budgetProjectionService.GetProjectionsForPlanAsync(activePlan.Id, ct);
                if (overview is not null)
                {
                    totalAllocated = overview.TotalAllocated;
                    totalSpent = overview.TotalSpent;
                }
            }

            var utilisation = totalAllocated > 0
                ? Math.Round(totalSpent / totalAllocated * 100, 1)
                : 0m;

            rows.Add(new ParticipantSummaryRow(
                Name: participant.FullName,
                NdisNumber: participant.NdisNumber,
                IsActive: participant.IsActive,
                ActivePlanNumber: activePlan?.PlanNumber,
                PlanEnd: activePlan?.EndDate,
                TotalAllocated: totalAllocated,
                TotalSpent: totalSpent,
                UtilisationPercent: utilisation));
        }

        return rows;
    }

    public async Task<IReadOnlyList<AuditTrailRow>> GetAuditTrailAsync(DateTime from, DateTime to, CancellationToken ct)
    {
        var events = await _eventStore.GetByDateRangeAsync(from, to, ct: ct);

        return events.Select(e => new AuditTrailRow(
            Timestamp: e.CreatedAt,
            StreamType: e.StreamType,
            EventType: e.EventType,
            StreamId: e.StreamId.ToString(),
            Details: TruncatePayload(e.Payload)
        )).ToList();
    }

    private static string TruncatePayload(string payload)
    {
        if (string.IsNullOrEmpty(payload)) return string.Empty;
        return payload.Length > 200 ? payload[..200] + "..." : payload;
    }
}
