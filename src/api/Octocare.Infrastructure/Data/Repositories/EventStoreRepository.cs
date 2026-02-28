using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Octocare.Application.Interfaces;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Repositories;

public class EventStoreRepository : IEventStore
{
    private readonly OctocareDbContext _db;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public EventStoreRepository(OctocareDbContext db)
    {
        _db = db;
    }

    public async Task AppendAsync(Guid streamId, string streamType, string eventType, object payload,
        int expectedVersion, object? metadata = null, CancellationToken ct = default)
    {
        // Optimistic concurrency check
        var currentVersion = await _db.Events
            .Where(e => e.StreamId == streamId)
            .MaxAsync(e => (int?)e.Version, ct) ?? 0;

        if (currentVersion != expectedVersion)
            throw new InvalidOperationException(
                $"Concurrency conflict on stream {streamId}. Expected version {expectedVersion}, but current version is {currentVersion}.");

        var newVersion = expectedVersion + 1;
        var payloadJson = JsonSerializer.Serialize(payload, JsonOptions);
        var metadataJson = metadata is not null ? JsonSerializer.Serialize(metadata, JsonOptions) : null;

        var storedEvent = StoredEvent.Create(streamId, streamType, eventType, payloadJson, newVersion, metadataJson);
        _db.Events.Add(storedEvent);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<StoredEventDto>> GetStreamAsync(Guid streamId, CancellationToken ct = default)
    {
        return await GetStreamAsync(streamId, 0, ct);
    }

    public async Task<IReadOnlyList<StoredEventDto>> GetStreamAsync(Guid streamId, int fromVersion, CancellationToken ct = default)
    {
        var events = await _db.Events
            .Where(e => e.StreamId == streamId && e.Version > fromVersion)
            .OrderBy(e => e.Version)
            .Select(e => new StoredEventDto(
                e.Id, e.StreamId, e.StreamType, e.EventType,
                e.Payload, e.Metadata, e.Version, e.CreatedAt.UtcDateTime))
            .ToListAsync(ct);

        return events;
    }

    public async Task<IReadOnlyList<StoredEventDto>> GetByDateRangeAsync(DateTime from, DateTime to, string? streamType = null, CancellationToken ct = default)
    {
        var fromOffset = new DateTimeOffset(from, TimeSpan.Zero);
        var toOffset = new DateTimeOffset(to, TimeSpan.Zero);

        var query = _db.Events
            .Where(e => e.CreatedAt >= fromOffset && e.CreatedAt <= toOffset);

        if (!string.IsNullOrWhiteSpace(streamType))
            query = query.Where(e => e.StreamType == streamType);

        var events = await query
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new StoredEventDto(
                e.Id, e.StreamId, e.StreamType, e.EventType,
                e.Payload, e.Metadata, e.Version, e.CreatedAt.UtcDateTime))
            .Take(1000)
            .ToListAsync(ct);

        return events;
    }
}
