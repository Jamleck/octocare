using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

public class StatementService
{
    private readonly IStatementRepository _statementRepo;
    private readonly IParticipantRepository _participantRepo;
    private readonly IPlanRepository _planRepo;
    private readonly IOrganisationRepository _orgRepo;
    private readonly IInvoiceRepository _invoiceRepo;
    private readonly IBudgetProjectionRepository _projectionRepo;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly StatementPdfGenerator _pdfGenerator;

    public StatementService(
        IStatementRepository statementRepo,
        IParticipantRepository participantRepo,
        IPlanRepository planRepo,
        IOrganisationRepository orgRepo,
        IInvoiceRepository invoiceRepo,
        IBudgetProjectionRepository projectionRepo,
        ICurrentUserService currentUser,
        IEmailSender emailSender,
        StatementPdfGenerator pdfGenerator)
    {
        _statementRepo = statementRepo;
        _participantRepo = participantRepo;
        _planRepo = planRepo;
        _orgRepo = orgRepo;
        _invoiceRepo = invoiceRepo;
        _projectionRepo = projectionRepo;
        _currentUser = currentUser;
        _emailSender = emailSender;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<StatementDto> GenerateStatementAsync(Guid participantId, GenerateStatementRequest request, CancellationToken ct)
    {
        var participant = await _participantRepo.GetByIdAsync(participantId, ct)
            ?? throw new KeyNotFoundException($"Participant {participantId} not found.");

        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(request.PlanId, ct)
            ?? throw new KeyNotFoundException($"Plan {request.PlanId} not found.");

        var tenantId = _currentUser.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        // Create statement record
        var statement = ParticipantStatement.Create(tenantId, participantId, plan.Id,
            request.PeriodStart, request.PeriodEnd);

        await _statementRepo.AddAsync(statement, ct);

        return MapToDto(statement);
    }

    public async Task<byte[]> GeneratePdfAsync(Guid statementId, CancellationToken ct)
    {
        var statement = await _statementRepo.GetByIdAsync(statementId, ct)
            ?? throw new KeyNotFoundException($"Statement {statementId} not found.");

        var data = await BuildStatementDataAsync(statement, ct);
        return _pdfGenerator.Generate(data);
    }

    public async Task<StatementDto> SendStatementAsync(Guid statementId, CancellationToken ct)
    {
        var statement = await _statementRepo.GetByIdAsync(statementId, ct)
            ?? throw new KeyNotFoundException($"Statement {statementId} not found.");

        var participant = statement.Participant;
        var recipientEmail = participant.Email ?? participant.NomineeEmail;
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new InvalidOperationException("Participant has no email address on file.");

        var data = await BuildStatementDataAsync(statement, ct);
        var pdf = _pdfGenerator.Generate(data);

        var subject = $"NDIS Statement - {participant.FullName} - {statement.PeriodStart:MMM yyyy}";
        var body = $"Please find attached your NDIS participant statement for the period {statement.PeriodStart:dd/MM/yyyy} to {statement.PeriodEnd:dd/MM/yyyy}.";
        var attachmentName = $"Statement_{participant.NdisNumber}_{statement.PeriodStart:yyyyMMdd}.pdf";

        await _emailSender.SendAsync(recipientEmail, subject, body, pdf, attachmentName, ct);

        statement.MarkSent();
        await _statementRepo.SaveAsync(ct);

        return MapToDto(statement);
    }

    public async Task<IReadOnlyList<StatementDto>> GetStatementsAsync(Guid participantId, CancellationToken ct)
    {
        var statements = await _statementRepo.GetByParticipantIdAsync(participantId, ct);
        return statements.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<StatementDto>> GenerateBatchAsync(CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");

        var activePlans = await _planRepo.GetActivePlansAsync(ct);
        var results = new List<StatementDto>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var periodStart = today.AddMonths(-1).AddDays(1 - today.Day); // First of previous month
        var periodEnd = periodStart.AddMonths(1).AddDays(-1); // Last of previous month

        foreach (var plan in activePlans)
        {
            var statement = ParticipantStatement.Create(tenantId, plan.ParticipantId, plan.Id,
                periodStart, periodEnd);
            await _statementRepo.AddAsync(statement, ct);
            results.Add(MapToDto(statement));
        }

        return results;
    }

    private async Task<StatementData> BuildStatementDataAsync(ParticipantStatement statement, CancellationToken ct)
    {
        var participant = statement.Participant;
        var plan = await _planRepo.GetByIdWithBudgetCategoriesAsync(statement.PlanId, ct)
            ?? throw new KeyNotFoundException($"Plan {statement.PlanId} not found.");

        // Get organisation
        var tenantId = _currentUser.TenantId
            ?? throw new InvalidOperationException("Tenant context is required.");
        var org = await _orgRepo.GetByIdAsync(tenantId, ct);

        // Get budget projections
        var projections = await _projectionRepo.GetByPlanIdAsync(plan.Id, ct);

        var budgetLines = new List<StatementBudgetLine>();
        foreach (var bc in plan.BudgetCategories)
        {
            var projection = projections.FirstOrDefault(p => p.BudgetCategoryId == bc.Id);
            var allocated = new Money(bc.AllocatedAmount).ToDollars();
            var spent = projection is not null ? new Money(projection.SpentAmount).ToDollars() : 0m;
            var available = allocated - spent;
            var utilisation = allocated > 0 ? Math.Round(spent / allocated * 100, 1) : 0m;

            budgetLines.Add(new StatementBudgetLine(
                bc.SupportCategory.ToString(),
                bc.SupportPurpose.ToString(),
                allocated, spent, available, utilisation));
        }

        // Get recent invoices for the period
        var (invoices, _) = await _invoiceRepo.GetPagedAsync(
            1, 50, null, statement.ParticipantId, null, ct);

        var recentInvoices = invoices
            .Where(i => i.PlanId == plan.Id)
            .OrderByDescending(i => i.ServicePeriodStart)
            .Take(20)
            .Select(i => new StatementInvoiceLine(
                i.ServicePeriodStart,
                i.Provider?.Name ?? "Unknown",
                $"Invoice {i.InvoiceNumber}",
                new Money(i.TotalAmount).ToDollars(),
                i.Status))
            .ToList();

        var totalAllocated = budgetLines.Sum(b => b.Allocated);
        var totalSpent = budgetLines.Sum(b => b.Spent);
        var totalAvailable = budgetLines.Sum(b => b.Available);

        return new StatementData(
            org?.Name ?? "Organisation",
            org?.ContactEmail,
            org?.ContactPhone,
            participant.FullName,
            participant.NdisNumber,
            plan.PlanNumber,
            plan.StartDate,
            plan.EndDate,
            statement.PeriodStart,
            statement.PeriodEnd,
            budgetLines,
            recentInvoices,
            totalAllocated,
            totalSpent,
            totalAvailable);
    }

    private static StatementDto MapToDto(ParticipantStatement statement) =>
        new(statement.Id,
            statement.ParticipantId,
            statement.PlanId,
            statement.PeriodStart,
            statement.PeriodEnd,
            statement.GeneratedAt,
            statement.SentAt);
}
