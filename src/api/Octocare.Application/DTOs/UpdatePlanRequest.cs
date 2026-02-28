namespace Octocare.Application.DTOs;

public record UpdatePlanRequest(
    string PlanNumber,
    DateOnly StartDate,
    DateOnly EndDate);
