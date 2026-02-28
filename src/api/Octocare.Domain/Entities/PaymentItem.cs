namespace Octocare.Domain.Entities;

public class PaymentItem
{
    public Guid Id { get; private set; }
    public Guid PaymentBatchId { get; private set; }
    public Guid ProviderId { get; private set; }
    public string ProviderName { get; private set; } = string.Empty;
    public long Amount { get; private set; } // cents
    public string InvoiceIds { get; private set; } = string.Empty; // comma-separated
    public string? RemittanceUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public PaymentBatch PaymentBatch { get; private set; } = null!;
    public Provider Provider { get; private set; } = null!;

    private PaymentItem() { }

    public static PaymentItem Create(Guid paymentBatchId, Guid providerId, string providerName, long amount, string invoiceIds)
    {
        return new PaymentItem
        {
            Id = Guid.NewGuid(),
            PaymentBatchId = paymentBatchId,
            ProviderId = providerId,
            ProviderName = providerName,
            Amount = amount,
            InvoiceIds = invoiceIds,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
