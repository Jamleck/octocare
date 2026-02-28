using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class ClaimTests
{
    private static Claim CreateDraft()
    {
        var claim = Claim.Create(Guid.NewGuid(), "CLM-20250901-ABC123");
        // Add a line item so Submit() won't fail
        claim.LineItems.Add(ClaimLineItem.Create(claim.Id, Guid.NewGuid()));
        return claim;
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var claim = Claim.Create(tenantId, "CLM-20250901-TEST01");

        Assert.NotEqual(Guid.Empty, claim.Id);
        Assert.Equal(tenantId, claim.TenantId);
        Assert.Equal("CLM-20250901-TEST01", claim.BatchNumber);
        Assert.Equal(ClaimStatus.Draft, claim.Status);
        Assert.Equal(0, claim.TotalAmount);
        Assert.Null(claim.NdiaReference);
        Assert.Null(claim.SubmissionDate);
        Assert.Null(claim.ResponseDate);
        Assert.True(claim.IsActive);
    }

    [Fact]
    public void Submit_FromDraft_TransitionsToSubmitted()
    {
        var claim = CreateDraft();

        claim.Submit();

        Assert.Equal(ClaimStatus.Submitted, claim.Status);
        Assert.NotNull(claim.SubmissionDate);
    }

    [Fact]
    public void Submit_WithNoLineItems_Throws()
    {
        var claim = Claim.Create(Guid.NewGuid(), "CLM-EMPTY");

        Assert.Throws<InvalidOperationException>(() => claim.Submit());
    }

    [Fact]
    public void Submit_FromSubmitted_Throws()
    {
        var claim = CreateDraft();
        claim.Submit();

        Assert.Throws<InvalidOperationException>(() => claim.Submit());
    }

    [Fact]
    public void Accept_FromSubmitted_TransitionsToAccepted()
    {
        var claim = CreateDraft();
        claim.Submit();

        claim.Accept();

        Assert.Equal(ClaimStatus.Accepted, claim.Status);
        Assert.NotNull(claim.ResponseDate);
    }

    [Fact]
    public void Accept_FromDraft_Throws()
    {
        var claim = CreateDraft();

        Assert.Throws<InvalidOperationException>(() => claim.Accept());
    }

    [Fact]
    public void PartiallyReject_FromSubmitted_TransitionsToPartiallyRejected()
    {
        var claim = CreateDraft();
        claim.Submit();

        claim.PartiallyReject();

        Assert.Equal(ClaimStatus.PartiallyRejected, claim.Status);
        Assert.NotNull(claim.ResponseDate);
    }

    [Fact]
    public void PartiallyReject_FromDraft_Throws()
    {
        var claim = CreateDraft();

        Assert.Throws<InvalidOperationException>(() => claim.PartiallyReject());
    }

    [Fact]
    public void Reject_FromSubmitted_TransitionsToRejected()
    {
        var claim = CreateDraft();
        claim.Submit();

        claim.Reject();

        Assert.Equal(ClaimStatus.Rejected, claim.Status);
        Assert.NotNull(claim.ResponseDate);
    }

    [Fact]
    public void Reject_FromDraft_Throws()
    {
        var claim = CreateDraft();

        Assert.Throws<InvalidOperationException>(() => claim.Reject());
    }

    [Fact]
    public void SetNdiaReference_SetsValue()
    {
        var claim = CreateDraft();

        claim.SetNdiaReference("NDIA-REF-001");

        Assert.Equal("NDIA-REF-001", claim.NdiaReference);
    }
}
