using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ServiceAgreementConfiguration : IEntityTypeConfiguration<ServiceAgreement>
{
    public void Configure(EntityTypeBuilder<ServiceAgreement> builder)
    {
        builder.ToTable("service_agreements");
        builder.HasKey(sa => sa.Id);
        builder.Property(sa => sa.Id).HasColumnName("id");
        builder.Property(sa => sa.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(sa => sa.ParticipantId).HasColumnName("participant_id").IsRequired();
        builder.Property(sa => sa.ProviderId).HasColumnName("provider_id").IsRequired();
        builder.Property(sa => sa.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(sa => sa.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(sa => sa.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(sa => sa.EndDate).HasColumnName("end_date").IsRequired();
        builder.Property(sa => sa.SignedDocumentUrl).HasColumnName("signed_document_url").HasMaxLength(500);
        builder.Property(sa => sa.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(sa => sa.CreatedAt).HasColumnName("created_at");
        builder.Property(sa => sa.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(sa => new { sa.TenantId, sa.ParticipantId });

        builder.HasOne(sa => sa.Participant)
            .WithMany()
            .HasForeignKey(sa => sa.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.Provider)
            .WithMany()
            .HasForeignKey(sa => sa.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.Plan)
            .WithMany()
            .HasForeignKey(sa => sa.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sa => sa.Items)
            .WithOne(i => i.Agreement)
            .HasForeignKey(i => i.ServiceAgreementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sa => sa.Bookings)
            .WithOne(b => b.Agreement)
            .HasForeignKey(b => b.ServiceAgreementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
