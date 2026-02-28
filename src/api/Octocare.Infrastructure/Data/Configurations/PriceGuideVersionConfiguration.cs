using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class PriceGuideVersionConfiguration : IEntityTypeConfiguration<PriceGuideVersion>
{
    public void Configure(EntityTypeBuilder<PriceGuideVersion> builder)
    {
        builder.ToTable("price_guide_versions");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id");
        builder.Property(v => v.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
        builder.Property(v => v.EffectiveFrom).HasColumnName("effective_from").IsRequired();
        builder.Property(v => v.EffectiveTo).HasColumnName("effective_to").IsRequired();
        builder.Property(v => v.IsCurrent).HasColumnName("is_current").HasDefaultValue(false);
        builder.Property(v => v.CreatedAt).HasColumnName("created_at");
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at");

        builder.HasMany(v => v.Items)
            .WithOne(i => i.Version)
            .HasForeignKey(i => i.VersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.Name).IsUnique();
    }
}
