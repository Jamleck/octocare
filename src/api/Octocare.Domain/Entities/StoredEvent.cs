namespace Octocare.Domain.Entities;

/// <summary>
/// Represents a persisted event in the event store.
/// Events are not tenant-scoped — they are identified by stream_id
/// which relates to tenant-scoped aggregates.
/// </summary>
public class StoredEvent
{
    public Guid Id { get; private set; }
    public Guid StreamId { get; private set; }
    public string StreamType { get; private set; } = string.Empty;
    public string EventType { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public string? Metadata { get; private set; }
    public int Version { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private StoredEvent() { }

    public static StoredEvent Create(Guid streamId, string streamType, string eventType,
        string payload, int version, string? metadata = null)
    {
        return new StoredEvent
        {
            Id = Guid.NewGuid(),
            StreamId = streamId,
            StreamType = streamType,
            EventType = eventType,
            Payload = payload,
            Metadata = metadata,
            Version = version,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
