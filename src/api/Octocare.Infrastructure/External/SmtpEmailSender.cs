using Microsoft.Extensions.Logging;
using Octocare.Application.Interfaces;

namespace Octocare.Infrastructure.External;

/// <summary>
/// Mock implementation of IEmailSender for development.
/// Logs email details instead of actually sending.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(ILogger<SmtpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null, CancellationToken ct = default)
    {
        _logger.LogWarning("DEV: Would send email to {To}, subject: {Subject}, attachment: {AttachmentName}",
            to, subject, attachmentName);
        return Task.CompletedTask;
    }
}
