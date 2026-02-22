using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class OrganisationConfiguration : IEntityTypeConfiguration<Organisation>
{
    public void Configure(EntityTypeBuilder<Organisation> builder)
    {
        builder.ToTable("organisations");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(o => o.Name).HasColumnName("name").HasMaxLength(256).IsRequired();
        builder.Property(o => o.Abn).HasColumnName("abn").HasMaxLength(11);
        builder.Property(o => o.ContactEmail).HasColumnName("contact_email").HasMaxLength(256);
        builder.Property(o => o.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
        builder.Property(o => o.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(o => o.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(o => o.CreatedAt).HasColumnName("created_at");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(o => o.TenantId);
        builder.HasIndex(o => o.Abn).IsUnique().HasFilter("abn IS NOT NULL");
    }
}
