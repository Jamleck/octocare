using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("participants");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(p => p.NdisNumber).HasColumnName("ndis_number").HasMaxLength(9).IsRequired();
        builder.Property(p => p.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.DateOfBirth).HasColumnName("date_of_birth").IsRequired();
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(256);
        builder.Property(p => p.Phone).HasColumnName("phone").HasMaxLength(20);
        builder.Property(p => p.Address).HasColumnName("address").HasMaxLength(500);
        builder.Property(p => p.NomineeName).HasColumnName("nominee_name").HasMaxLength(200);
        builder.Property(p => p.NomineeEmail).HasColumnName("nominee_email").HasMaxLength(256);
        builder.Property(p => p.NomineePhone).HasColumnName("nominee_phone").HasMaxLength(20);
        builder.Property(p => p.NomineeRelationship).HasColumnName("nominee_relationship").HasMaxLength(50);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(p => new { p.TenantId, p.NdisNumber }).IsUnique();
        builder.HasIndex(p => p.TenantId);

        builder.Ignore(p => p.FullName);
    }
}
