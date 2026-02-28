using Octocare.Domain.Entities;

namespace Octocare.Application.Interfaces;

public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<EmailTemplate?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<EmailTemplate> AddAsync(EmailTemplate template, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken ct = default);
}
