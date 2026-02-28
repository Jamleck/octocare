using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class InvoiceTests
{
    private static Invoice CreateSubmitted()
    {
        return Invoice.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            "INV-001", new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 31));
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var invoice = Invoice.Create(tenantId, providerId, participantId, planId,
            "INV-TEST-001", new DateOnly(2025, 7, 1), new DateOnly(2025, 7, 31), "Some notes");

        Assert.NotEqual(Guid.Empty, invoice.Id);
        Assert.Equal(tenantId, invoice.TenantId);
        Assert.Equal(providerId, invoice.ProviderId);
        Assert.Equal(participantId, invoice.ParticipantId);
        Assert.Equal(planId, invoice.PlanId);
        Assert.Equal("INV-TEST-001", invoice.InvoiceNumber);
        Assert.Equal(new DateOnly(2025, 7, 1), invoice.ServicePeriodStart);
        Assert.Equal(new DateOnly(2025, 7, 31), invoice.ServicePeriodEnd);
        Assert.Equal(0, invoice.TotalAmount);
        Assert.Equal(InvoiceStatus.Submitted, invoice.Status);
        Assert.Equal("manual_entry", invoice.Source);
        Assert.Equal("Some notes", invoice.Notes);
        Assert.True(invoice.IsActive);
    }

    [Fact]
    public void SubmitForReview_FromSubmitted_TransitionsToUnderReview()
    {
        var invoice = CreateSubmitted();

        invoice.SubmitForReview();

        Assert.Equal(InvoiceStatus.UnderReview, invoice.Status);
    }

    [Fact]
    public void SubmitForReview_FromApproved_Throws()
    {
        var invoice = CreateSubmitted();
        invoice.Approve();

        Assert.Throws<InvalidOperationException>(() => invoice.SubmitForReview());
    }

    [Fact]
    public void Approve_FromSubmitted_TransitionsToApproved()
    {
        var invoice = CreateSubmitted();

        invoice.Approve();

        Assert.Equal(InvoiceStatus.Approved, invoice.Status);
    }

    [Fact]
    public void Approve_FromUnderReview_TransitionsToApproved()
    {
        var invoice = CreateSubmitted();
        invoice.SubmitForReview();

        invoice.Approve();

        Assert.Equal(InvoiceStatus.Approved, invoice.Status);
    }

    [Fact]
    public void Approve_FromRejected_Throws()
    {
        var invoice = CreateSubmitted();
        invoice.Reject("bad data");

        Assert.Throws<InvalidOperationException>(() => invoice.Approve());
    }

    [Fact]
    public void Reject_FromSubmitted_TransitionsToRejected()
    {
        var invoice = CreateSubmitted();

        invoice.Reject("Duplicate invoice");

        Assert.Equal(InvoiceStatus.Rejected, invoice.Status);
        Assert.Equal("Duplicate invoice", invoice.Notes);
    }

    [Fact]
    public void Reject_FromUnderReview_TransitionsToRejected()
    {
        var invoice = CreateSubmitted();
        invoice.SubmitForReview();

        invoice.Reject("Incorrect amounts");

        Assert.Equal(InvoiceStatus.Rejected, invoice.Status);
    }

    [Fact]
    public void Reject_FromApproved_Throws()
    {
        var invoice = CreateSubmitted();
        invoice.Approve();

        Assert.Throws<InvalidOperationException>(() => invoice.Reject("reason"));
    }

    [Fact]
    public void Dispute_FromSubmitted_TransitionsToDisputed()
    {
        var invoice = CreateSubmitted();

        invoice.Dispute("Rate seems too high");

        Assert.Equal(InvoiceStatus.Disputed, invoice.Status);
        Assert.Equal("Rate seems too high", invoice.Notes);
    }

    [Fact]
    public void Dispute_FromApproved_TransitionsToDisputed()
    {
        var invoice = CreateSubmitted();
        invoice.Approve();

        invoice.Dispute("Participant raised concern");

        Assert.Equal(InvoiceStatus.Disputed, invoice.Status);
    }

    [Fact]
    public void Dispute_FromPaid_Throws()
    {
        var invoice = CreateSubmitted();
        invoice.Approve();
        invoice.MarkPaid();

        Assert.Throws<InvalidOperationException>(() => invoice.Dispute("reason"));
    }

    [Fact]
    public void MarkPaid_FromApproved_TransitionsToPaid()
    {
        var invoice = CreateSubmitted();
        invoice.Approve();

        invoice.MarkPaid();

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public void MarkPaid_FromSubmitted_Throws()
    {
        var invoice = CreateSubmitted();

        Assert.Throws<InvalidOperationException>(() => invoice.MarkPaid());
    }

    [Fact]
    public void RecalculateTotal_SumsLineItems()
    {
        var invoice = CreateSubmitted();
        var li1 = InvoiceLineItem.Create(invoice.Id, "01_002_0107_1_1", "Desc 1",
            new DateOnly(2025, 7, 7), 2m, 5000);
        var li2 = InvoiceLineItem.Create(invoice.Id, "01_015_0107_1_1", "Desc 2",
            new DateOnly(2025, 7, 14), 3m, 8000);

        invoice.LineItems.Add(li1);
        invoice.LineItems.Add(li2);
        invoice.RecalculateTotal();

        // 2 * 5000 + 3 * 8000 = 10000 + 24000 = 34000
        Assert.Equal(34000, invoice.TotalAmount);
    }
}
