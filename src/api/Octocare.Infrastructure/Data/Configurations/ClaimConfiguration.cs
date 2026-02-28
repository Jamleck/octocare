using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToTable("claims");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(c => c.BatchNumber).HasColumnName("batch_number").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(c => c.TotalAmount).HasColumnName("total_amount").IsRequired();
        builder.Property(c => c.NdiaReference).HasColumnName("ndia_reference").HasMaxLength(100);
        builder.Property(c => c.SubmissionDate).HasColumnName("submission_date");
        builder.Property(c => c.ResponseDate).HasColumnName("response_date");
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => new { c.TenantId, c.BatchNumber }).IsUnique();

        builder.HasMany(c => c.LineItems)
            .WithOne(li => li.Claim)
            .HasForeignKey(li => li.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
