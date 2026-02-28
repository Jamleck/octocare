using Octocare.Domain.Enums;

namespace Octocare.Domain.Entities;

public class PaymentBatch
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string BatchNumber { get; private set; } = string.Empty;
    public string Status { get; private set; } = PaymentBatchStatus.Draft;
    public long TotalAmount { get; private set; } // cents
    public string? AbaFileUrl { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? SentAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }

    public ICollection<PaymentItem> Items { get; private set; } = new List<PaymentItem>();

    private PaymentBatch() { }

    public static PaymentBatch Create(Guid tenantId, string batchNumber)
    {
        return new PaymentBatch
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BatchNumber = batchNumber,
            Status = PaymentBatchStatus.Draft,
            TotalAmount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void AddItem(PaymentItem item)
    {
        Items.Add(item);
        RecalculateTotal();
    }

    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Amount);
    }

    public void MarkGenerated(string? abaFileUrl = null)
    {
        if (Status != PaymentBatchStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot generate ABA for a batch with status '{Status}'. Batch must be in Draft status.");

        Status = PaymentBatchStatus.Generated;
        AbaFileUrl = abaFileUrl;
    }

    public void MarkSent()
    {
        if (Status != PaymentBatchStatus.Generated)
            throw new InvalidOperationException(
                $"Cannot mark as sent a batch with status '{Status}'. Batch must be in Generated status.");

        Status = PaymentBatchStatus.Sent;
        SentAt = DateTimeOffset.UtcNow;
    }

    public void MarkConfirmed()
    {
        if (Status != PaymentBatchStatus.Sent)
            throw new InvalidOperationException(
                $"Cannot confirm a batch with status '{Status}'. Batch must be in Sent status.");

        Status = PaymentBatchStatus.Confirmed;
        ConfirmedAt = DateTimeOffset.UtcNow;
    }
}
