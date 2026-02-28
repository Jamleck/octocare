namespace Octocare.Domain.Entities;

public static class ClaimLineItemStatus
{
    public const string Pending = "pending";
    public const string Accepted = "accepted";
    public const string Rejected = "rejected";
}

public class ClaimLineItem
{
    public Guid Id { get; private set; }
    public Guid ClaimId { get; private set; }
    public Guid InvoiceLineItemId { get; private set; }
    public string Status { get; private set; } = ClaimLineItemStatus.Pending;
    public string? RejectionReason { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Claim Claim { get; private set; } = null!;
    public InvoiceLineItem InvoiceLineItem { get; private set; } = null!;

    private ClaimLineItem() { }

    public static ClaimLineItem Create(Guid claimId, Guid invoiceLineItemId)
    {
        return new ClaimLineItem
        {
            Id = Guid.NewGuid(),
            ClaimId = claimId,
            InvoiceLineItemId = invoiceLineItemId,
            Status = ClaimLineItemStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Accept()
    {
        if (Status != ClaimLineItemStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot accept a claim line item with status '{Status}'. Item must be in Pending status.");

        Status = ClaimLineItemStatus.Accepted;
    }

    public void Reject(string reason)
    {
        if (Status != ClaimLineItemStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot reject a claim line item with status '{Status}'. Item must be in Pending status.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A rejection reason is required.", nameof(reason));

        Status = ClaimLineItemStatus.Rejected;
        RejectionReason = reason;
    }
}
