using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Octocare.Domain.Entities;

namespace Octocare.Infrastructure.Data.Configurations;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("invoice_line_items");
        builder.HasKey(li => li.Id);
        builder.Property(li => li.Id).HasColumnName("id");
        builder.Property(li => li.InvoiceId).HasColumnName("invoice_id").IsRequired();
        builder.Property(li => li.SupportItemNumber).HasColumnName("support_item_number").HasMaxLength(50).IsRequired();
        builder.Property(li => li.Description).HasColumnName("description").HasMaxLength(500).IsRequired();
        builder.Property(li => li.ServiceDate).HasColumnName("service_date").IsRequired();
        builder.Property(li => li.Quantity).HasColumnName("quantity").HasColumnType("decimal(18,4)").IsRequired();
        builder.Property(li => li.Rate).HasColumnName("rate").IsRequired();
        builder.Property(li => li.Amount).HasColumnName("amount").IsRequired();
        builder.Property(li => li.BudgetCategoryId).HasColumnName("budget_category_id");
        builder.Property(li => li.ValidationStatus).HasColumnName("validation_status").HasMaxLength(20).IsRequired();
        builder.Property(li => li.ValidationMessage).HasColumnName("validation_message").HasMaxLength(500);
        builder.Property(li => li.CreatedAt).HasColumnName("created_at");

        builder.HasOne(li => li.Invoice)
            .WithMany(i => i.LineItems)
            .HasForeignKey(li => li.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(li => li.BudgetCategory)
            .WithMany()
            .HasForeignKey(li => li.BudgetCategoryId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
