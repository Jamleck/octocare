using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Domain;

public class PaymentBatchTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var batch = PaymentBatch.Create(tenantId, "PAY-20260228-ABC123");

        Assert.NotEqual(Guid.Empty, batch.Id);
        Assert.Equal(tenantId, batch.TenantId);
        Assert.Equal("PAY-20260228-ABC123", batch.BatchNumber);
        Assert.Equal(PaymentBatchStatus.Draft, batch.Status);
        Assert.Equal(0, batch.TotalAmount);
        Assert.Null(batch.AbaFileUrl);
        Assert.Null(batch.SentAt);
        Assert.Null(batch.ConfirmedAt);
        Assert.Empty(batch.Items);
    }

    [Fact]
    public void AddItem_IncreasesTotalAmount()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");
        var item = PaymentItem.Create(batch.Id, Guid.NewGuid(), "Provider A", 150000, "inv1,inv2");

        batch.AddItem(item);

        Assert.Single(batch.Items);
        Assert.Equal(150000, batch.TotalAmount);
    }

    [Fact]
    public void AddItem_MultipleItems_SumsCorrectly()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");
        batch.AddItem(PaymentItem.Create(batch.Id, Guid.NewGuid(), "Provider A", 100000, "inv1"));
        batch.AddItem(PaymentItem.Create(batch.Id, Guid.NewGuid(), "Provider B", 250000, "inv2,inv3"));

        Assert.Equal(2, batch.Items.Count);
        Assert.Equal(350000, batch.TotalAmount);
    }

    [Fact]
    public void MarkGenerated_FromDraft_TransitionsToGenerated()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");

        batch.MarkGenerated("https://example.com/aba");

        Assert.Equal(PaymentBatchStatus.Generated, batch.Status);
        Assert.Equal("https://example.com/aba", batch.AbaFileUrl);
    }

    [Fact]
    public void MarkGenerated_FromGenerated_Throws()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");
        batch.MarkGenerated();

        Assert.Throws<InvalidOperationException>(() => batch.MarkGenerated());
    }

    [Fact]
    public void MarkSent_FromGenerated_TransitionsToSent()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");
        batch.MarkGenerated();

        batch.MarkSent();

        Assert.Equal(PaymentBatchStatus.Sent, batch.Status);
        Assert.NotNull(batch.SentAt);
    }

    [Fact]
    public void MarkSent_FromDraft_Throws()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");

        Assert.Throws<InvalidOperationException>(() => batch.MarkSent());
    }

    [Fact]
    public void MarkConfirmed_FromSent_TransitionsToConfirmed()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");
        batch.MarkGenerated();
        batch.MarkSent();

        batch.MarkConfirmed();

        Assert.Equal(PaymentBatchStatus.Confirmed, batch.Status);
        Assert.NotNull(batch.ConfirmedAt);
    }

    [Fact]
    public void MarkConfirmed_FromDraft_Throws()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");

        Assert.Throws<InvalidOperationException>(() => batch.MarkConfirmed());
    }

    [Fact]
    public void MarkConfirmed_FromGenerated_Throws()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");
        batch.MarkGenerated();

        Assert.Throws<InvalidOperationException>(() => batch.MarkConfirmed());
    }
}
