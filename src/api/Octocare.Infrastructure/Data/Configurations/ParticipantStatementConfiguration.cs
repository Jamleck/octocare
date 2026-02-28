using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ParticipantStatementConfiguration : IEntityTypeConfiguration<ParticipantStatement>
{
    public void Configure(EntityTypeBuilder<ParticipantStatement> builder)
    {
        builder.ToTable("participant_statements");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(s => s.ParticipantId).HasColumnName("participant_id").IsRequired();
        builder.Property(s => s.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(s => s.PeriodStart).HasColumnName("period_start").IsRequired();
        builder.Property(s => s.PeriodEnd).HasColumnName("period_end").IsRequired();
        builder.Property(s => s.GeneratedAt).HasColumnName("generated_at").IsRequired();
        builder.Property(s => s.SentAt).HasColumnName("sent_at");
        builder.Property(s => s.PdfUrl).HasColumnName("pdf_url").HasMaxLength(500);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");

        builder.HasOne(s => s.Participant)
            .WithMany()
            .HasForeignKey(s => s.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.TenantId, s.ParticipantId });
    }
}
