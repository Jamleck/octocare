using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(i => i.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(i => i.ParticipantId).HasColumnName("participant_id").IsRequired();
        builder.Property(i => i.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(i => i.InvoiceNumber).HasColumnName("invoice_number").HasMaxLength(100).IsRequired();
        builder.Property(i => i.ServicePeriodStart).HasColumnName("service_period_start").IsRequired();
        builder.Property(i => i.ServicePeriodEnd).HasColumnName("service_period_end").IsRequired();
        builder.Property(i => i.TotalAmount).HasColumnName("total_amount").IsRequired();
        builder.Property(i => i.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(i => i.Source).HasColumnName("source").HasMaxLength(30).IsRequired();
        builder.Property(i => i.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(i => i.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");
        builder.Property(i => i.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(i => new { i.TenantId, i.ProviderId });
        builder.HasIndex(i => new { i.TenantId, i.ParticipantId });
        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();

        builder.HasOne(i => i.Provider)
            .WithMany()
            .HasForeignKey(i => i.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Participant)
            .WithMany()
            .HasForeignKey(i => i.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.Plan)
            .WithMany()
            .HasForeignKey(i => i.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.LineItems)
            .WithOne(li => li.Invoice)
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
