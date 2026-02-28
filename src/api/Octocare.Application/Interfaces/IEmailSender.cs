namespace Octocare.Application.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachmentName = null, CancellationToken ct = default);
}
