namespace Octocare.Application.DTOs;

public record CreatePlanRequest(
    string PlanNumber,
    DateOnly StartDate,
    DateOnly EndDate);
