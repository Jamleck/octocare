using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class ServiceAgreementTests
{
    private static ServiceAgreement CreateDraft()
    {
        return ServiceAgreement.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var participantId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var agreement = ServiceAgreement.Create(tenantId, participantId, providerId, planId,
            new DateOnly(2025, 7, 1), new DateOnly(2026, 6, 30));

        Assert.NotEqual(Guid.Empty, agreement.Id);
        Assert.Equal(tenantId, agreement.TenantId);
        Assert.Equal(participantId, agreement.ParticipantId);
        Assert.Equal(providerId, agreement.ProviderId);
        Assert.Equal(planId, agreement.PlanId);
        Assert.Equal(ServiceAgreementStatus.Draft, agreement.Status);
        Assert.Equal(new DateOnly(2025, 7, 1), agreement.StartDate);
        Assert.Equal(new DateOnly(2026, 6, 30), agreement.EndDate);
        Assert.True(agreement.IsActive);
        Assert.Null(agreement.SignedDocumentUrl);
    }

    [Fact]
    public void Send_FromDraft_TransitionsToSent()
    {
        var agreement = CreateDraft();

        agreement.Send();

        Assert.Equal(ServiceAgreementStatus.Sent, agreement.Status);
    }

    [Fact]
    public void Send_FromNonDraft_Throws()
    {
        var agreement = CreateDraft();
        agreement.Send();

        Assert.Throws<InvalidOperationException>(() => agreement.Send());
    }

    [Fact]
    public void Activate_FromDraft_TransitionsToActive()
    {
        var agreement = CreateDraft();

        agreement.Activate();

        Assert.Equal(ServiceAgreementStatus.Active, agreement.Status);
    }

    [Fact]
    public void Activate_FromSent_TransitionsToActive()
    {
        var agreement = CreateDraft();
        agreement.Send();

        agreement.Activate();

        Assert.Equal(ServiceAgreementStatus.Active, agreement.Status);
    }

    [Fact]
    public void Activate_FromActive_Throws()
    {
        var agreement = CreateDraft();
        agreement.Activate();

        Assert.Throws<InvalidOperationException>(() => agreement.Activate());
    }

    [Fact]
    public void Expire_FromActive_TransitionsToExpired()
    {
        var agreement = CreateDraft();
        agreement.Activate();

        agreement.Expire();

        Assert.Equal(ServiceAgreementStatus.Expired, agreement.Status);
        Assert.False(agreement.IsActive);
    }

    [Fact]
    public void Expire_FromDraft_Throws()
    {
        var agreement = CreateDraft();

        Assert.Throws<InvalidOperationException>(() => agreement.Expire());
    }

    [Fact]
    public void Terminate_FromActive_TransitionsToTerminated()
    {
        var agreement = CreateDraft();
        agreement.Activate();

        agreement.Terminate();

        Assert.Equal(ServiceAgreementStatus.Terminated, agreement.Status);
        Assert.False(agreement.IsActive);
    }

    [Fact]
    public void Terminate_FromDraft_Throws()
    {
        var agreement = CreateDraft();

        Assert.Throws<InvalidOperationException>(() => agreement.Terminate());
    }

    [Fact]
    public void Update_InDraftStatus_ModifiesFields()
    {
        var agreement = CreateDraft();

        agreement.Update(new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), "https://docs.example.com/signed.pdf");

        Assert.Equal(new DateOnly(2025, 8, 1), agreement.StartDate);
        Assert.Equal(new DateOnly(2026, 7, 31), agreement.EndDate);
        Assert.Equal("https://docs.example.com/signed.pdf", agreement.SignedDocumentUrl);
    }

    [Fact]
    public void Update_InActiveStatus_Throws()
    {
        var agreement = CreateDraft();
        agreement.Activate();

        Assert.Throws<InvalidOperationException>(() =>
            agreement.Update(new DateOnly(2025, 8, 1), new DateOnly(2026, 7, 31), null));
    }
}
