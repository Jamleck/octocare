namespace Octocare.Application.Interfaces;

public interface IEventStore
{
    Task AppendAsync(Guid streamId, string streamType, string eventType, object payload, int expectedVersion, object? metadata = null, CancellationToken ct = default);
    Task<IReadOnlyList<StoredEventDto>> GetStreamAsync(Guid streamId, CancellationToken ct = default);
    Task<IReadOnlyList<StoredEventDto>> GetStreamAsync(Guid streamId, int fromVersion, CancellationToken ct = default);
}

public record StoredEventDto(Guid Id, Guid StreamId, string StreamType, string EventType, string Payload, string? Metadata, int Version, DateTime CreatedAt);
