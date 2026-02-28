using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;
using Octocare.Domain.Enums;

namespace Octocare.Infrastructure.Data.Configurations;

public class BudgetCategoryConfiguration : IEntityTypeConfiguration<BudgetCategory>
{
    public void Configure(EntityTypeBuilder<BudgetCategory> builder)
    {
        builder.ToTable("budget_categories");
        builder.HasKey(bc => bc.Id);
        builder.Property(bc => bc.Id).HasColumnName("id");
        builder.Property(bc => bc.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(bc => bc.SupportCategory).HasColumnName("support_category")
            .HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(bc => bc.SupportPurpose).HasColumnName("support_purpose")
            .HasConversion<string>().HasMaxLength(60).IsRequired();
        builder.Property(bc => bc.AllocatedAmount).HasColumnName("allocated_amount").IsRequired();
        builder.Property(bc => bc.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(bc => bc.CreatedAt).HasColumnName("created_at");
        builder.Property(bc => bc.UpdatedAt).HasColumnName("updated_at");

        builder.HasOne(bc => bc.Plan)
            .WithMany(p => p.BudgetCategories)
            .HasForeignKey(bc => bc.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
