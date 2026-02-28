using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IPaymentRepository
{
    Task<PaymentBatch?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<PaymentBatch> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? status = null,
        CancellationToken ct = default);
    Task<PaymentBatch> AddAsync(PaymentBatch batch, CancellationToken ct = default);
    Task UpdateAsync(PaymentBatch batch, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
