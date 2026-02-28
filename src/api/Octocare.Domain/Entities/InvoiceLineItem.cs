namespace Octocare.Domain.Entities;

public static class LineItemValidationStatus
{
    public const string Valid = "valid";
    public const string Warning = "warning";
    public const string Error = "error";
}

public class InvoiceLineItem
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string SupportItemNumber { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateOnly ServiceDate { get; private set; }
    public decimal Quantity { get; private set; }
    public long Rate { get; private set; } // cents per unit
    public long Amount { get; private set; } // cents (quantity * rate, computed)
    public Guid? BudgetCategoryId { get; private set; }
    public string ValidationStatus { get; private set; } = LineItemValidationStatus.Valid;
    public string? ValidationMessage { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Invoice Invoice { get; private set; } = null!;
    public BudgetCategory? BudgetCategory { get; private set; }

    private InvoiceLineItem() { }

    public static InvoiceLineItem Create(Guid invoiceId, string supportItemNumber, string description,
        DateOnly serviceDate, decimal quantity, long rateCents, Guid? budgetCategoryId = null)
    {
        var amount = (long)decimal.Round(quantity * rateCents, MidpointRounding.ToEven);

        return new InvoiceLineItem
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            SupportItemNumber = supportItemNumber,
            Description = description,
            ServiceDate = serviceDate,
            Quantity = quantity,
            Rate = rateCents,
            Amount = amount,
            BudgetCategoryId = budgetCategoryId,
            ValidationStatus = LineItemValidationStatus.Valid,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateValidation(string status, string? message)
    {
        ValidationStatus = status;
        ValidationMessage = message;
    }
}
