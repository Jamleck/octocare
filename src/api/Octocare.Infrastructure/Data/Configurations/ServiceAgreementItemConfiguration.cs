using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ServiceAgreementItemConfiguration : IEntityTypeConfiguration<ServiceAgreementItem>
{
    public void Configure(EntityTypeBuilder<ServiceAgreementItem> builder)
    {
        builder.ToTable("service_agreement_items");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.ServiceAgreementId).HasColumnName("service_agreement_id").IsRequired();
        builder.Property(i => i.SupportItemNumber).HasColumnName("support_item_number").HasMaxLength(50).IsRequired();
        builder.Property(i => i.AgreedRate).HasColumnName("agreed_rate").IsRequired();
        builder.Property(i => i.Frequency).HasColumnName("frequency").HasMaxLength(30);
        builder.Property(i => i.CreatedAt).HasColumnName("created_at");

        builder.HasOne(i => i.Agreement)
            .WithMany(sa => sa.Items)
            .HasForeignKey(i => i.ServiceAgreementId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
