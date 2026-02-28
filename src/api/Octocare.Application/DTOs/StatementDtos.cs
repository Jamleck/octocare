namespace Octocare.Application.DTOs;

public record StatementData(
    string OrganisationName,
    string? OrganisationEmail,
    string? OrganisationPhone,
    string ParticipantName,
    string NdisNumber,
    string PlanNumber,
    DateOnly PlanStart,
    DateOnly PlanEnd,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    List<StatementBudgetLine> BudgetLines,
    List<StatementInvoiceLine> RecentInvoices,
    decimal TotalAllocated,
    decimal TotalSpent,
    decimal TotalAvailable);

public record StatementBudgetLine(
    string Category,
    string Purpose,
    decimal Allocated,
    decimal Spent,
    decimal Available,
    decimal UtilisationPercent);

public record StatementInvoiceLine(
    DateOnly ServiceDate,
    string ProviderName,
    string Description,
    decimal Amount,
    string Status);

public record StatementDto(
    Guid Id,
    Guid ParticipantId,
    Guid PlanId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateTimeOffset GeneratedAt,
    DateTimeOffset? SentAt);

public record GenerateStatementRequest(
    Guid PlanId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd);
