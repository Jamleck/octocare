using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class ParticipantTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var tenantId = Guid.NewGuid();
        var participant = Participant.Create(
            tenantId, "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15), "sarah@test.com", "0412345678",
            "123 Main St", "Margaret Johnson", "margaret@test.com",
            "0498765432", "Mother");

        Assert.NotEqual(Guid.Empty, participant.Id);
        Assert.Equal(tenantId, participant.TenantId);
        Assert.Equal("431234567", participant.NdisNumber);
        Assert.Equal("Sarah Johnson", participant.FullName);
        Assert.Equal(new DateOnly(1985, 3, 15), participant.DateOfBirth);
        Assert.Equal("Margaret Johnson", participant.NomineeName);
        Assert.Equal("Mother", participant.NomineeRelationship);
        Assert.True(participant.IsActive);
    }

    [Fact]
    public void Update_ModifiesMutableFields()
    {
        var participant = Participant.Create(Guid.NewGuid(), "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15));

        participant.Update("Jane", "Smith", new DateOnly(1990, 1, 1),
            "jane@test.com", "0400000000", "456 Oak Ave",
            "Bob Smith", "bob@test.com", "0411111111", "Spouse");

        Assert.Equal("Jane Smith", participant.FullName);
        Assert.Equal(new DateOnly(1990, 1, 1), participant.DateOfBirth);
        Assert.Equal("Bob Smith", participant.NomineeName);
        Assert.Equal("431234567", participant.NdisNumber); // unchanged
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var participant = Participant.Create(Guid.NewGuid(), "431234567", "Sarah", "Johnson",
            new DateOnly(1985, 3, 15));
        participant.Deactivate();
        Assert.False(participant.IsActive);
    }
}
