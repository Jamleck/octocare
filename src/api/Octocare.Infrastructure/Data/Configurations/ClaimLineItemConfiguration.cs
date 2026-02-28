using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ClaimLineItemConfiguration : IEntityTypeConfiguration<ClaimLineItem>
{
    public void Configure(EntityTypeBuilder<ClaimLineItem> builder)
    {
        builder.ToTable("claim_line_items");
        builder.HasKey(li => li.Id);
        builder.Property(li => li.Id).HasColumnName("id");
        builder.Property(li => li.ClaimId).HasColumnName("claim_id").IsRequired();
        builder.Property(li => li.InvoiceLineItemId).HasColumnName("invoice_line_item_id").IsRequired();
        builder.Property(li => li.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(li => li.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
        builder.Property(li => li.CreatedAt).HasColumnName("created_at");

        builder.HasOne(li => li.Claim)
            .WithMany(c => c.LineItems)
            .HasForeignKey(li => li.ClaimId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(li => li.InvoiceLineItem)
            .WithMany()
            .HasForeignKey(li => li.InvoiceLineItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
