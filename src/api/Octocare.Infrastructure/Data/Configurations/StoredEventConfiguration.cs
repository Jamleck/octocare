using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.StreamId).HasColumnName("stream_id").IsRequired();
        builder.Property(e => e.StreamType).HasColumnName("stream_type").HasMaxLength(200).IsRequired();
        builder.Property(e => e.EventType).HasColumnName("event_type").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Payload).HasColumnName("payload").HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb");
        builder.Property(e => e.Version).HasColumnName("version").IsRequired();
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(e => new { e.StreamId, e.Version }).IsUnique();
        builder.HasIndex(e => e.StreamId);
        builder.HasIndex(e => e.EventType);
    }
}
