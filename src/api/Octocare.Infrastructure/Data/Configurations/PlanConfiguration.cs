using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(p => p.ParticipantId).HasColumnName("participant_id").IsRequired();
        builder.Property(p => p.PlanNumber).HasColumnName("plan_number").HasMaxLength(50).IsRequired();
        builder.Property(p => p.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(p => p.EndDate).HasColumnName("end_date").IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(20).IsRequired();
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(p => new { p.TenantId, p.ParticipantId });
        builder.HasIndex(p => new { p.TenantId, p.PlanNumber }).IsUnique();

        builder.HasOne(p => p.Participant)
            .WithMany()
            .HasForeignKey(p => p.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.BudgetCategories)
            .WithOne(bc => bc.Plan)
            .HasForeignKey(bc => bc.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
