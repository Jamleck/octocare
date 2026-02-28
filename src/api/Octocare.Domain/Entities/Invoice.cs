namespace Octocare.Domain.Entities;

public static class InvoiceStatus
{
    public const string Submitted = "submitted";
    public const string UnderReview = "under_review";
    public const string Approved = "approved";
    public const string Rejected = "rejected";
    public const string Disputed = "disputed";
    public const string Paid = "paid";

    public static readonly string[] ValidStatuses =
        [Submitted, UnderReview, Approved, Rejected, Disputed, Paid];
}

public class Invoice
{
    public Guid Id { get; private set; }
    public Guid TenantId { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid ParticipantId { get; private set; }
    public Guid PlanId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateOnly ServicePeriodStart { get; private set; }
    public DateOnly ServicePeriodEnd { get; private set; }
    public long TotalAmount { get; private set; } // cents, computed from line items
    public string Status { get; private set; } = InvoiceStatus.Submitted;
    public string Source { get; private set; } = "manual_entry";
    public string? Notes { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public Provider Provider { get; private set; } = null!;
    public Participant Participant { get; private set; } = null!;
    public Plan Plan { get; private set; } = null!;
    public ICollection<InvoiceLineItem> LineItems { get; private set; } = new List<InvoiceLineItem>();

    private Invoice() { }

    public static Invoice Create(Guid tenantId, Guid providerId, Guid participantId, Guid planId,
        string invoiceNumber, DateOnly servicePeriodStart, DateOnly servicePeriodEnd, string? notes = null)
    {
        return new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProviderId = providerId,
            ParticipantId = participantId,
            PlanId = planId,
            InvoiceNumber = invoiceNumber,
            ServicePeriodStart = servicePeriodStart,
            ServicePeriodEnd = servicePeriodEnd,
            TotalAmount = 0,
            Status = InvoiceStatus.Submitted,
            Source = "manual_entry",
            Notes = notes,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void SubmitForReview()
    {
        if (Status != InvoiceStatus.Submitted)
            throw new InvalidOperationException(
                $"Cannot submit for review an invoice with status '{Status}'. Invoice must be in Submitted status.");

        Status = InvoiceStatus.UnderReview;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Approve()
    {
        if (Status != InvoiceStatus.Submitted && Status != InvoiceStatus.UnderReview)
            throw new InvalidOperationException(
                $"Cannot approve an invoice with status '{Status}'. Invoice must be in Submitted or Under Review status.");

        Status = InvoiceStatus.Approved;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reject(string reason)
    {
        if (Status != InvoiceStatus.Submitted && Status != InvoiceStatus.UnderReview)
            throw new InvalidOperationException(
                $"Cannot reject an invoice with status '{Status}'. Invoice must be in Submitted or Under Review status.");

        Notes = reason;
        Status = InvoiceStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Dispute(string reason)
    {
        if (Status != InvoiceStatus.Approved && Status != InvoiceStatus.Submitted && Status != InvoiceStatus.UnderReview)
            throw new InvalidOperationException(
                $"Cannot dispute an invoice with status '{Status}'. Invoice must be in Submitted, Under Review, or Approved status.");

        Notes = reason;
        Status = InvoiceStatus.Disputed;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void MarkPaid()
    {
        if (Status != InvoiceStatus.Approved)
            throw new InvalidOperationException(
                $"Cannot mark as paid an invoice with status '{Status}'. Invoice must be in Approved status.");

        Status = InvoiceStatus.Paid;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetSource(string source)
    {
        Source = source;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void RecalculateTotal()
    {
        TotalAmount = LineItems.Sum(li => li.Amount);
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
