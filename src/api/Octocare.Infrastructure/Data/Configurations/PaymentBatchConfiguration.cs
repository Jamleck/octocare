using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class PaymentBatchConfiguration : IEntityTypeConfiguration<PaymentBatch>
{
    public void Configure(EntityTypeBuilder<PaymentBatch> builder)
    {
        builder.ToTable("payment_batches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(b => b.BatchNumber).HasColumnName("batch_number").HasMaxLength(100).IsRequired();
        builder.Property(b => b.Status).HasColumnName("status").HasMaxLength(30).IsRequired();
        builder.Property(b => b.TotalAmount).HasColumnName("total_amount").IsRequired();
        builder.Property(b => b.AbaFileUrl).HasColumnName("aba_file_url").HasMaxLength(500);
        builder.Property(b => b.CreatedAt).HasColumnName("created_at");
        builder.Property(b => b.SentAt).HasColumnName("sent_at");
        builder.Property(b => b.ConfirmedAt).HasColumnName("confirmed_at");

        builder.HasIndex(b => new { b.TenantId, b.BatchNumber }).IsUnique();

        builder.HasMany(b => b.Items)
            .WithOne(i => i.PaymentBatch)
            .HasForeignKey(i => i.PaymentBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
