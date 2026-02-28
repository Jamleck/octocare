using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class SupportItemConfiguration : IEntityTypeConfiguration<SupportItem>
{
    public void Configure(EntityTypeBuilder<SupportItem> builder)
    {
        builder.ToTable("support_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.VersionId).HasColumnName("version_id").IsRequired();
        builder.Property(i => i.ItemNumber).HasColumnName("item_number").HasMaxLength(30).IsRequired();
        builder.Property(i => i.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(i => i.SupportCategory).HasColumnName("support_category")
            .HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(i => i.SupportPurpose).HasColumnName("support_purpose")
            .HasConversion<string>().HasMaxLength(60).IsRequired();
        builder.Property(i => i.Unit).HasColumnName("unit")
            .HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(i => i.PriceLimitNational).HasColumnName("price_limit_national");
        builder.Property(i => i.PriceLimitRemote).HasColumnName("price_limit_remote");
        builder.Property(i => i.PriceLimitVeryRemote).HasColumnName("price_limit_very_remote");
        builder.Property(i => i.IsTtpEligible).HasColumnName("is_ttp_eligible").HasDefaultValue(false);
        builder.Property(i => i.CancellationRule).HasColumnName("cancellation_rule")
            .HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.ClaimType).HasColumnName("claim_type")
            .HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(i => new { i.VersionId, i.ItemNumber }).IsUnique();
        builder.HasIndex(i => i.VersionId);
    }
}
