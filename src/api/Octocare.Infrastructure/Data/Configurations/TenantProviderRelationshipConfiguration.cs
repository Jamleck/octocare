using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class TenantProviderRelationshipConfiguration : IEntityTypeConfiguration<TenantProviderRelationship>
{
    public void Configure(EntityTypeBuilder<TenantProviderRelationship> builder)
    {
        builder.ToTable("tenant_provider_relationships");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(r => r.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(r => r.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(r => new { r.TenantId, r.ProviderId }).IsUnique();
        builder.HasIndex(r => r.TenantId);
    }
}
