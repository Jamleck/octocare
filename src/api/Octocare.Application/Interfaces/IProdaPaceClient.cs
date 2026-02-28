using Octocare.Application.DTOs;

namespace Octocare.Application.Interfaces;

public interface IProdaPaceClient
{
    Task<ProdaPlanInfo?> GetPlanInfoAsync(string ndisNumber, CancellationToken ct = default);
    Task<ProdaBudgetInfo?> GetBudgetInfoAsync(string ndisNumber, string planId, CancellationToken ct = default);
    Task<ProdaParticipantInfo?> GetParticipantInfoAsync(string ndisNumber, CancellationToken ct = default);
    Task<bool> VerifyPlanAsync(string ndisNumber, string planNumber, CancellationToken ct = default);
}
