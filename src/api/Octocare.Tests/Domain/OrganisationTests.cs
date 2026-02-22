using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class OrganisationTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var org = Organisation.Create("Test Org", "51824753556", "test@org.com", "0400000000", "123 St");

        Assert.NotEqual(Guid.Empty, org.Id);
        Assert.Equal(org.Id, org.TenantId); // org IS the tenant
        Assert.Equal("Test Org", org.Name);
        Assert.Equal("51824753556", org.Abn);
        Assert.Equal("test@org.com", org.ContactEmail);
        Assert.Equal("0400000000", org.ContactPhone);
        Assert.Equal("123 St", org.Address);
        Assert.True(org.IsActive);
    }

    [Fact]
    public void Update_ModifiesMutableFields()
    {
        var org = Organisation.Create("Original");
        var createdAt = org.CreatedAt;

        org.Update("Updated", "51824753556", "new@org.com", "0411111111", "456 Ave");

        Assert.Equal("Updated", org.Name);
        Assert.Equal("51824753556", org.Abn);
        Assert.Equal("new@org.com", org.ContactEmail);
        Assert.Equal("0411111111", org.ContactPhone);
        Assert.Equal("456 Ave", org.Address);
        Assert.Equal(createdAt, org.CreatedAt); // unchanged
    }
}
