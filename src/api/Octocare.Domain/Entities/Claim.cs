namespace Octocare.Domain.Entities;

public static class ClaimStatus
{
    public const string Draft = "draft";
    public const string Submitted = "submitted";
    public const string Accepted = "accepted";
    public const string PartiallyRejected = "partially_rejected";
    public const string Rejected = "rejected";

    public static readonly string[] ValidStatuses =
        [Draft, Submitted, Accepted, PartiallyRejected, Rejected];
}

public class Claim
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public string BatchNumber { get; private set; } = string.Empty;
    public string Status { get; private set; } = ClaimStatus.Draft;
    public long TotalAmount { get; private set; } // cents
    public string? NdiaReference { get; private set; }
    public DateOnly? SubmissionDate { get; private set; }
    public DateOnly? ResponseDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<ClaimLineItem> LineItems { get; private set; } = new List<ClaimLineItem>();

    private Claim() { }

    public static Claim Create(Guid tenantId, string batchNumber)
    {
        return new Claim
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            BatchNumber = batchNumber,
            Status = ClaimStatus.Draft,
            TotalAmount = 0,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Submit()
    {
        if (Status != ClaimStatus.Draft)
            throw new InvalidOperationException(
                $"Cannot submit a claim with status '{Status}'. Claim must be in Draft status.");

        if (!LineItems.Any())
            throw new InvalidOperationException("Cannot submit a claim with no line items.");

        Status = ClaimStatus.Submitted;
        SubmissionDate = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Accept()
    {
        if (Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot accept a claim with status '{Status}'. Claim must be in Submitted status.");

        Status = ClaimStatus.Accepted;
        ResponseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void PartiallyReject()
    {
        if (Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot partially reject a claim with status '{Status}'. Claim must be in Submitted status.");

        Status = ClaimStatus.PartiallyRejected;
        ResponseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject()
    {
        if (Status != ClaimStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot reject a claim with status '{Status}'. Claim must be in Submitted status.");

        Status = ClaimStatus.Rejected;
        ResponseDate = DateOnly.FromDateTime(DateTime.UtcNow);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetNdiaReference(string reference)
    {
        NdiaReference = reference;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecalculateTotal()
    {
        TotalAmount = LineItems.Sum(li => li.InvoiceLineItem?.Amount ?? 0);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
