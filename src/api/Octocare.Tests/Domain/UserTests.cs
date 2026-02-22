using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var user = User.Create("auth0|123", "test@test.com", "John", "Doe");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("auth0|123", user.ExternalId);
        Assert.Equal("test@test.com", user.Email);
        Assert.Equal("John", user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Equal("John Doe", user.FullName);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var user = User.Create("auth0|123", "test@test.com", "John", "Doe");
        user.Deactivate();
        Assert.False(user.IsActive);
    }

    [Fact]
    public void Update_ModifiesName()
    {
        var user = User.Create("auth0|123", "test@test.com", "John", "Doe");
        user.Update("Jane", "Smith");
        Assert.Equal("Jane Smith", user.FullName);
    }
}
