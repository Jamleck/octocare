using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Tests.Domain;

public class SupportItemTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var versionId = Guid.NewGuid();

        var item = SupportItem.Create(
            versionId,
            "01_011_0107_1_1",
            "Assistance with Daily Life Activities in a Group",
            SupportCategory.Core,
            SupportPurpose.DailyActivities,
            UnitOfMeasure.Hour,
            6547,
            8184,
            9820,
            true,
            CancellationRule.ShortNotice2Day,
            ClaimType.Time);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(versionId, item.VersionId);
        Assert.Equal("01_011_0107_1_1", item.ItemNumber);
        Assert.Equal("Assistance with Daily Life Activities in a Group", item.Name);
        Assert.Equal(SupportCategory.Core, item.SupportCategory);
        Assert.Equal(SupportPurpose.DailyActivities, item.SupportPurpose);
        Assert.Equal(UnitOfMeasure.Hour, item.Unit);
        Assert.Equal(6547, item.PriceLimitNational);
        Assert.Equal(8184, item.PriceLimitRemote);
        Assert.Equal(9820, item.PriceLimitVeryRemote);
        Assert.True(item.IsTtpEligible);
        Assert.Equal(CancellationRule.ShortNotice2Day, item.CancellationRule);
        Assert.Equal(ClaimType.Time, item.ClaimType);
    }

    [Fact]
    public void Create_NonTimeItem_SetsCorrectClaimType()
    {
        var item = SupportItem.Create(
            Guid.NewGuid(),
            "15_037_0117_1_3",
            "Plan Management Monthly Fee",
            SupportCategory.CapacityBuilding,
            SupportPurpose.ImprovedDailyLivingSkills,
            UnitOfMeasure.Each,
            3628,
            3628,
            3628,
            false,
            CancellationRule.None,
            ClaimType.NonTime);

        Assert.Equal(ClaimType.NonTime, item.ClaimType);
        Assert.Equal(CancellationRule.None, item.CancellationRule);
        Assert.False(item.IsTtpEligible);
        Assert.Equal(UnitOfMeasure.Each, item.Unit);
    }
}
