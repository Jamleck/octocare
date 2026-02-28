using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Invoice> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        string? status = null, Guid? participantId = null, Guid? providerId = null,
        CancellationToken ct = default);
    Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber, Guid? excludeId = null, CancellationToken ct = default);
    Task<Invoice> AddAsync(Invoice invoice, CancellationToken ct = default);
    Task UpdateAsync(Invoice invoice, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}
