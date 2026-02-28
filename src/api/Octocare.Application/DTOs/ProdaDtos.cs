namespace Octocare.Application.DTOs;

public record ProdaPlanInfo(
    string PlanNumber,
    string Status,
    DateOnly StartDate,
    DateOnly EndDate,
    decimal TotalBudget);

public record ProdaBudgetInfo(
    string PlanNumber,
    List<ProdaBudgetLine> Categories);

public record ProdaBudgetLine(
    string Category,
    string Purpose,
    decimal Allocated,
    decimal Used,
    decimal Available);

public record ProdaParticipantInfo(
    string NdisNumber,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string? Phone,
    string? Email);
