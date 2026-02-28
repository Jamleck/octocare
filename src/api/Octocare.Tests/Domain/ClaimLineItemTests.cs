using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class ClaimLineItemTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var claimId = Guid.NewGuid();
        var invoiceLineItemId = Guid.NewGuid();

        var lineItem = ClaimLineItem.Create(claimId, invoiceLineItemId);

        Assert.NotEqual(Guid.Empty, lineItem.Id);
        Assert.Equal(claimId, lineItem.ClaimId);
        Assert.Equal(invoiceLineItemId, lineItem.InvoiceLineItemId);
        Assert.Equal(ClaimLineItemStatus.Pending, lineItem.Status);
        Assert.Null(lineItem.RejectionReason);
    }

    [Fact]
    public void Accept_FromPending_TransitionsToAccepted()
    {
        var lineItem = ClaimLineItem.Create(Guid.NewGuid(), Guid.NewGuid());

        lineItem.Accept();

        Assert.Equal(ClaimLineItemStatus.Accepted, lineItem.Status);
    }

    [Fact]
    public void Accept_FromAccepted_Throws()
    {
        var lineItem = ClaimLineItem.Create(Guid.NewGuid(), Guid.NewGuid());
        lineItem.Accept();

        Assert.Throws<InvalidOperationException>(() => lineItem.Accept());
    }

    [Fact]
    public void Reject_FromPending_TransitionsToRejected()
    {
        var lineItem = ClaimLineItem.Create(Guid.NewGuid(), Guid.NewGuid());

        lineItem.Reject("Rate exceeds price limit");

        Assert.Equal(ClaimLineItemStatus.Rejected, lineItem.Status);
        Assert.Equal("Rate exceeds price limit", lineItem.RejectionReason);
    }

    [Fact]
    public void Reject_FromRejected_Throws()
    {
        var lineItem = ClaimLineItem.Create(Guid.NewGuid(), Guid.NewGuid());
        lineItem.Reject("reason");

        Assert.Throws<InvalidOperationException>(() => lineItem.Reject("another reason"));
    }

    [Fact]
    public void Reject_WithEmptyReason_Throws()
    {
        var lineItem = ClaimLineItem.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => lineItem.Reject(""));
    }

    [Fact]
    public void Reject_WithWhitespaceReason_Throws()
    {
        var lineItem = ClaimLineItem.Create(Guid.NewGuid(), Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => lineItem.Reject("   "));
    }
}
