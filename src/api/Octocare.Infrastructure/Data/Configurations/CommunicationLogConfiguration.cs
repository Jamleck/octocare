using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class CommunicationLogConfiguration : IEntityTypeConfiguration<CommunicationLog>
{
    public void Configure(EntityTypeBuilder<CommunicationLog> builder)
    {
        builder.ToTable("communication_logs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(c => c.RecipientEmail).HasColumnName("recipient_email").HasMaxLength(320).IsRequired();
        builder.Property(c => c.Subject).HasColumnName("subject").HasMaxLength(500).IsRequired();
        builder.Property(c => c.Body).HasColumnName("body").IsRequired();
        builder.Property(c => c.TemplateName).HasColumnName("template_name").HasMaxLength(100);
        builder.Property(c => c.SentAt).HasColumnName("sent_at");
        builder.Property(c => c.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(c => c.ErrorMessage).HasColumnName("error_message").HasMaxLength(1000);
        builder.Property(c => c.RelatedEntityType).HasColumnName("related_entity_type").HasMaxLength(100);
        builder.Property(c => c.RelatedEntityId).HasColumnName("related_entity_id").HasMaxLength(100);

        builder.HasIndex(c => new { c.TenantId, c.SentAt });
        builder.HasIndex(c => c.RecipientEmail);
    }
}
