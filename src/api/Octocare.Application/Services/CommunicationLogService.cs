using Octocare.Application.DTOs;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

public class CommunicationLogService
{
    private readonly ICommunicationLogRepository _logRepo;
    private readonly ITenantContext _tenantContext;

    public CommunicationLogService(ICommunicationLogRepository logRepo, ITenantContext tenantContext)
    {
        _logRepo = logRepo;
        _tenantContext = tenantContext;
    }

    public async Task LogAsync(string recipientEmail, string subject, string body, string status,
        string? templateName = null, string? relatedEntityType = null, string? relatedEntityId = null,
        string? errorMessage = null, CancellationToken ct = default)
    {
        var tenantId = _tenantContext.TenantId
            ?? throw new InvalidOperationException("No tenant context.");

        var log = CommunicationLog.Create(tenantId, recipientEmail, subject, body, status,
            templateName, relatedEntityType, relatedEntityId, errorMessage);
        await _logRepo.AddAsync(log, ct);
    }

    public async Task<CommunicationLogPagedResult> GetLogsAsync(int page = 1, int pageSize = 20,
        string? recipientEmail = null, string? templateName = null, CancellationToken ct = default)
    {
        var (items, totalCount) = await _logRepo.GetPagedAsync(page, pageSize, recipientEmail, templateName, ct);
        return new CommunicationLogPagedResult(
            items.Select(MapToDto).ToList(),
            totalCount,
            page,
            pageSize);
    }

    private static CommunicationLogDto MapToDto(CommunicationLog log)
    {
        return new CommunicationLogDto(
            log.Id,
            log.RecipientEmail,
            log.Subject,
            log.TemplateName,
            log.SentAt,
            log.Status,
            log.ErrorMessage,
            log.RelatedEntityType,
            log.RelatedEntityId);
    }
}
