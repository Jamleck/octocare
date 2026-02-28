using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class ProviderTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var provider = Provider.Create(
            "Allied Health Plus", "53004085616",
            "contact@ahp.com.au", "0290001111",
            "10 George St, Sydney NSW 2000");

        Assert.NotEqual(Guid.Empty, provider.Id);
        Assert.Equal("Allied Health Plus", provider.Name);
        Assert.Equal("53004085616", provider.Abn);
        Assert.Equal("contact@ahp.com.au", provider.ContactEmail);
        Assert.Equal("0290001111", provider.ContactPhone);
        Assert.Equal("10 George St, Sydney NSW 2000", provider.Address);
        Assert.True(provider.IsActive);
    }

    [Fact]
    public void Create_WithOptionalNulls_Succeeds()
    {
        var provider = Provider.Create("Minimal Provider");

        Assert.NotEqual(Guid.Empty, provider.Id);
        Assert.Equal("Minimal Provider", provider.Name);
        Assert.Null(provider.Abn);
        Assert.Null(provider.ContactEmail);
        Assert.Null(provider.ContactPhone);
        Assert.Null(provider.Address);
        Assert.True(provider.IsActive);
    }

    [Fact]
    public void Update_ModifiesMutableFields()
    {
        var provider = Provider.Create("Original Name", "53004085616");

        provider.Update("Updated Name", "72004085616",
            "new@email.com", "0400111222", "New Address");

        Assert.Equal("Updated Name", provider.Name);
        Assert.Equal("72004085616", provider.Abn);
        Assert.Equal("new@email.com", provider.ContactEmail);
        Assert.Equal("0400111222", provider.ContactPhone);
        Assert.Equal("New Address", provider.Address);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var provider = Provider.Create("Test Provider");
        provider.Deactivate();
        Assert.False(provider.IsActive);
    }
}
