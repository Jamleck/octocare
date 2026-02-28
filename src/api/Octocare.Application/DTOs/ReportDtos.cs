namespace Octocare.Application.DTOs;

public record BudgetUtilisationReportRow(
    string ParticipantName,
    string NdisNumber,
    string PlanNumber,
    string Category,
    string Purpose,
    decimal Allocated,
    decimal Spent,
    decimal Available,
    decimal UtilisationPercent);

public record OutstandingInvoiceRow(
    string InvoiceNumber,
    string ProviderName,
    string ParticipantName,
    DateOnly ServicePeriodEnd,
    decimal Amount,
    string Status,
    int DaysOutstanding,
    string AgeBucket);

public record ClaimStatusRow(
    string BatchNumber,
    string Status,
    decimal TotalAmount,
    int LineItemCount,
    int AcceptedCount,
    int RejectedCount,
    DateOnly? SubmissionDate);

public record ParticipantSummaryRow(
    string Name,
    string NdisNumber,
    bool IsActive,
    string? ActivePlanNumber,
    DateOnly? PlanEnd,
    decimal TotalAllocated,
    decimal TotalSpent,
    decimal UtilisationPercent);

public record AuditTrailRow(
    DateTime Timestamp,
    string StreamType,
    string EventType,
    string StreamId,
    string Details);
