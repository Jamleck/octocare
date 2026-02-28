using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(p => p.Abn).HasColumnName("abn").HasMaxLength(11);
        builder.Property(p => p.ContactEmail).HasColumnName("contact_email").HasMaxLength(256);
        builder.Property(p => p.ContactPhone).HasColumnName("contact_phone").HasMaxLength(20);
        builder.Property(p => p.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        // No global query filter — providers are shared across tenants
    }
}
