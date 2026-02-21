namespace Octocare.Domain.Events;

/// <summary>
/// Base class for all domain events in the event-sourced system.
/// </summary>
public abstract record DomainEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid StreamId { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public string EventType => GetType().Name;
}
