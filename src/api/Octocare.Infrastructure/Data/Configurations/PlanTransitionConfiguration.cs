using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class PlanTransitionConfiguration : IEntityTypeConfiguration<PlanTransition>
{
    public void Configure(EntityTypeBuilder<PlanTransition> builder)
    {
        builder.ToTable("plan_transitions");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");
        builder.Property(t => t.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(t => t.OldPlanId).HasColumnName("old_plan_id").IsRequired();
        builder.Property(t => t.NewPlanId).HasColumnName("new_plan_id");
        builder.Property(t => t.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(t => t.ChecklistItems).HasColumnName("checklist_items").HasColumnType("text").IsRequired();
        builder.Property(t => t.Notes).HasColumnName("notes").HasMaxLength(2000);
        builder.Property(t => t.CreatedAt).HasColumnName("created_at");
        builder.Property(t => t.CompletedAt).HasColumnName("completed_at");

        builder.HasOne(t => t.OldPlan)
            .WithMany()
            .HasForeignKey(t => t.OldPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.NewPlan)
            .WithMany()
            .HasForeignKey(t => t.NewPlanId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => new { t.TenantId, t.OldPlanId });
    }
}
