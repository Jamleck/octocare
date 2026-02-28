namespace Octocare.Application.DTOs;

public record GenerateMonthlyFeesRequest(int Month, int Year);

public record GenerateMonthlyFeesResponse(int InvoicesGenerated, List<Guid> InvoiceIds);

public record GenerateSetupFeeResponse(Guid InvoiceId);
