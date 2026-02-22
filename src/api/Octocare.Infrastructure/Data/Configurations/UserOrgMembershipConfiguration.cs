using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class UserOrgMembershipConfiguration : IEntityTypeConfiguration<UserOrgMembership>
{
    public void Configure(EntityTypeBuilder<UserOrgMembership> builder)
    {
        builder.ToTable("user_org_memberships");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(m => m.OrganisationId).HasColumnName("organisation_id").IsRequired();
        builder.Property(m => m.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(m => m.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
        builder.Property(m => m.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(m => new { m.UserId, m.OrganisationId }).IsUnique();
        builder.HasIndex(m => m.TenantId);

        builder.HasOne(m => m.User)
            .WithMany(u => u.Memberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Organisation)
            .WithMany(o => o.Memberships)
            .HasForeignKey(m => m.OrganisationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
