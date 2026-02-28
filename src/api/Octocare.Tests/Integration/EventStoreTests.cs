using Microsoft.Extensions.DependencyInjection;
using Octocare.Application.Interfaces;

namespace Octocare.Tests.Integration;

public class EventStoreTests : IntegrationTestBase
{
    [Fact]
    public async Task AppendAndGetStream_RoundTrips()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var streamId = Guid.NewGuid();

        // Act — append two events
        await eventStore.AppendAsync(streamId, "TestStream", "TestCreated",
            new { Name = "Test", Value = 42 }, expectedVersion: 0);
        await eventStore.AppendAsync(streamId, "TestStream", "TestUpdated",
            new { Name = "Updated", Value = 99 }, expectedVersion: 1);

        // Assert — read the full stream
        var events = await eventStore.GetStreamAsync(streamId);
        Assert.Equal(2, events.Count);
        Assert.Equal("TestCreated", events[0].EventType);
        Assert.Equal("TestUpdated", events[1].EventType);
        Assert.Equal(1, events[0].Version);
        Assert.Equal(2, events[1].Version);
        Assert.Equal(streamId, events[0].StreamId);
        Assert.Contains("\"name\":\"Test\"", events[0].Payload);
        Assert.Contains("\"name\":\"Updated\"", events[1].Payload);
    }

    [Fact]
    public async Task GetStream_FromVersion_ReturnsSubset()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var streamId = Guid.NewGuid();

        await eventStore.AppendAsync(streamId, "TestStream", "Event1",
            new { Order = 1 }, expectedVersion: 0);
        await eventStore.AppendAsync(streamId, "TestStream", "Event2",
            new { Order = 2 }, expectedVersion: 1);
        await eventStore.AppendAsync(streamId, "TestStream", "Event3",
            new { Order = 3 }, expectedVersion: 2);

        // Act — read from version 2 onwards
        var events = await eventStore.GetStreamAsync(streamId, fromVersion: 2);

        // Assert
        Assert.Single(events);
        Assert.Equal("Event3", events[0].EventType);
        Assert.Equal(3, events[0].Version);
    }

    [Fact]
    public async Task Append_WithWrongVersion_ThrowsConcurrencyException()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
        var streamId = Guid.NewGuid();

        await eventStore.AppendAsync(streamId, "TestStream", "Event1",
            new { Order = 1 }, expectedVersion: 0);

        // Act & Assert — wrong expected version
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            eventStore.AppendAsync(streamId, "TestStream", "Event2",
                new { Order = 2 }, expectedVersion: 0));
    }

    [Fact]
    public async Task GetStream_EmptyStream_ReturnsEmptyList()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        // Act
        var events = await eventStore.GetStreamAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(events);
    }
}
