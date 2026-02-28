using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ServiceBookingConfiguration : IEntityTypeConfiguration<ServiceBooking>
{
    public void Configure(EntityTypeBuilder<ServiceBooking> builder)
    {
        builder.ToTable("service_bookings");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("id");
        builder.Property(b => b.ServiceAgreementId).HasColumnName("service_agreement_id").IsRequired();
        builder.Property(b => b.BudgetCategoryId).HasColumnName("budget_category_id").IsRequired();
        builder.Property(b => b.AllocatedAmount).HasColumnName("allocated_amount").IsRequired();
        builder.Property(b => b.UsedAmount).HasColumnName("used_amount").IsRequired();
        builder.Property(b => b.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(b => b.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(b => b.CreatedAt).HasColumnName("created_at");
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(b => b.Agreement)
            .WithMany(sa => sa.Bookings)
            .HasForeignKey(b => b.ServiceAgreementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.BudgetCategory)
            .WithMany()
            .HasForeignKey(b => b.BudgetCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
