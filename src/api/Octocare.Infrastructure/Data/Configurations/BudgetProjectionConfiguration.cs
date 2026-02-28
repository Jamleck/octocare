using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class BudgetProjectionConfiguration : IEntityTypeConfiguration<BudgetProjection>
{
    public void Configure(EntityTypeBuilder<BudgetProjection> builder)
    {
        builder.ToTable("budget_projections");
        builder.HasKey(bp => bp.Id);
        builder.Property(bp => bp.Id).HasColumnName("id");
        builder.Property(bp => bp.BudgetCategoryId).HasColumnName("budget_category_id").IsRequired();
        builder.Property(bp => bp.AllocatedAmount).HasColumnName("allocated_amount").IsRequired();
        builder.Property(bp => bp.CommittedAmount).HasColumnName("committed_amount").IsRequired();
        builder.Property(bp => bp.SpentAmount).HasColumnName("spent_amount").IsRequired();
        builder.Property(bp => bp.PendingAmount).HasColumnName("pending_amount").IsRequired();
        builder.Property(bp => bp.CreatedAt).HasColumnName("created_at");
        builder.Property(bp => bp.UpdatedAt).HasColumnName("updated_at");

        // AvailableAmount is computed — ignore from EF mapping
        builder.Ignore(bp => bp.AvailableAmount);

        builder.HasOne(bp => bp.BudgetCategory)
            .WithMany()
            .HasForeignKey(bp => bp.BudgetCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
