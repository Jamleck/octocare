using Octocare.Domain.Events;

namespace Octocare.Application.Interfaces;

public interface IEventStore
{
    Task AppendAsync(string streamType, Guid streamId, IReadOnlyList<DomainEvent> events, long expectedVersion, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DomainEvent>> ReadStreamAsync(Guid streamId, CancellationToken cancellationToken = default);
}
