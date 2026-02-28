using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class BudgetAlertConfiguration : IEntityTypeConfiguration<BudgetAlert>
{
    public void Configure(EntityTypeBuilder<BudgetAlert> builder)
    {
        builder.ToTable("budget_alerts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(a => a.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(a => a.BudgetCategoryId).HasColumnName("budget_category_id");
        builder.Property(a => a.AlertType).HasColumnName("alert_type").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(a => a.Severity).HasColumnName("severity").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(a => a.Message).HasColumnName("message").HasMaxLength(500).IsRequired();
        builder.Property(a => a.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(a => a.IsDismissed).HasColumnName("is_dismissed").HasDefaultValue(false);
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.ReadAt).HasColumnName("read_at");
        builder.Property(a => a.Data).HasColumnName("data");

        builder.HasOne(a => a.Plan)
            .WithMany()
            .HasForeignKey(a => a.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.BudgetCategory)
            .WithMany()
            .HasForeignKey(a => a.BudgetCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => new { a.TenantId, a.PlanId });
        builder.HasIndex(a => new { a.TenantId, a.IsRead, a.IsDismissed });
    }
}
