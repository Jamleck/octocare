using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class ServiceBookingTests
{
    private static ServiceBooking CreateActiveBooking(long allocatedCents = 1000000)
    {
        return ServiceBooking.Create(Guid.NewGuid(), Guid.NewGuid(), allocatedCents);
    }

    [Fact]
    public void Create_SetsAllProperties()
    {
        var agreementId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var booking = ServiceBooking.Create(agreementId, categoryId, 500000);

        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.Equal(agreementId, booking.ServiceAgreementId);
        Assert.Equal(categoryId, booking.BudgetCategoryId);
        Assert.Equal(500000, booking.AllocatedAmount);
        Assert.Equal(0, booking.UsedAmount);
        Assert.Equal(ServiceBookingStatus.Active, booking.Status);
        Assert.True(booking.IsActive);
    }

    [Fact]
    public void Cancel_FromActive_TransitionsToCancelled()
    {
        var booking = CreateActiveBooking();

        booking.Cancel();

        Assert.Equal(ServiceBookingStatus.Cancelled, booking.Status);
        Assert.False(booking.IsActive);
    }

    [Fact]
    public void Cancel_FromCancelled_Throws()
    {
        var booking = CreateActiveBooking();
        booking.Cancel();

        Assert.Throws<InvalidOperationException>(() => booking.Cancel());
    }

    [Fact]
    public void Cancel_FromCompleted_Throws()
    {
        var booking = CreateActiveBooking();
        booking.Complete();

        Assert.Throws<InvalidOperationException>(() => booking.Cancel());
    }

    [Fact]
    public void RecordUsage_ValidAmount_IncreasesUsed()
    {
        var booking = CreateActiveBooking(1000000); // $10,000.00

        booking.RecordUsage(250000); // $2,500.00

        Assert.Equal(250000, booking.UsedAmount);
    }

    [Fact]
    public void RecordUsage_MultipleRecords_AccumulatesUsed()
    {
        var booking = CreateActiveBooking(1000000);

        booking.RecordUsage(250000);
        booking.RecordUsage(300000);

        Assert.Equal(550000, booking.UsedAmount);
    }

    [Fact]
    public void RecordUsage_ExceedsAllocated_Throws()
    {
        var booking = CreateActiveBooking(100000); // $1,000.00

        Assert.Throws<InvalidOperationException>(() => booking.RecordUsage(200000)); // $2,000.00
    }

    [Fact]
    public void RecordUsage_ZeroAmount_Throws()
    {
        var booking = CreateActiveBooking();

        Assert.Throws<ArgumentException>(() => booking.RecordUsage(0));
    }

    [Fact]
    public void RecordUsage_NegativeAmount_Throws()
    {
        var booking = CreateActiveBooking();

        Assert.Throws<ArgumentException>(() => booking.RecordUsage(-100));
    }

    [Fact]
    public void RecordUsage_OnCancelledBooking_Throws()
    {
        var booking = CreateActiveBooking();
        booking.Cancel();

        Assert.Throws<InvalidOperationException>(() => booking.RecordUsage(100));
    }

    [Fact]
    public void Complete_FromActive_TransitionsToCompleted()
    {
        var booking = CreateActiveBooking();

        booking.Complete();

        Assert.Equal(ServiceBookingStatus.Completed, booking.Status);
        Assert.False(booking.IsActive);
    }

    [Fact]
    public void Complete_FromCancelled_Throws()
    {
        var booking = CreateActiveBooking();
        booking.Cancel();

        Assert.Throws<InvalidOperationException>(() => booking.Complete());
    }
}
