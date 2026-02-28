using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class PaymentItemTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var batchId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var item = PaymentItem.Create(batchId, providerId, "Provider A", 150000, "inv1,inv2,inv3");

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(batchId, item.PaymentBatchId);
        Assert.Equal(providerId, item.ProviderId);
        Assert.Equal("Provider A", item.ProviderName);
        Assert.Equal(150000, item.Amount);
        Assert.Equal("inv1,inv2,inv3", item.InvoiceIds);
        Assert.Null(item.RemittanceUrl);
    }
}
