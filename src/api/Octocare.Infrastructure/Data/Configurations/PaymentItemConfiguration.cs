using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class PaymentItemConfiguration : IEntityTypeConfiguration<PaymentItem>
{
    public void Configure(EntityTypeBuilder<PaymentItem> builder)
    {
        builder.ToTable("payment_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.PaymentBatchId).HasColumnName("payment_batch_id").IsRequired();
        builder.Property(i => i.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(i => i.ProviderName).HasColumnName("provider_name").HasMaxLength(200).IsRequired();
        builder.Property(i => i.Amount).HasColumnName("amount").IsRequired();
        builder.Property(i => i.InvoiceIds).HasColumnName("invoice_ids").HasMaxLength(2000).IsRequired();
        builder.Property(i => i.RemittanceUrl).HasColumnName("remittance_url").HasMaxLength(500);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");

        builder.HasOne(i => i.Provider)
            .WithMany()
            .HasForeignKey(i => i.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
