using Octocare.Domain.Entities;

namespace Octocare.Tests.Domain;

public class InvoiceLineItemTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var invoiceId = Guid.NewGuid();
        var budgetCategoryId = Guid.NewGuid();

        var lineItem = InvoiceLineItem.Create(
            invoiceId, "01_002_0107_1_1", "Assistance with Self-Care",
            new DateOnly(2025, 7, 7), 2.5m, 8445, budgetCategoryId);

        Assert.NotEqual(Guid.Empty, lineItem.Id);
        Assert.Equal(invoiceId, lineItem.InvoiceId);
        Assert.Equal("01_002_0107_1_1", lineItem.SupportItemNumber);
        Assert.Equal("Assistance with Self-Care", lineItem.Description);
        Assert.Equal(new DateOnly(2025, 7, 7), lineItem.ServiceDate);
        Assert.Equal(2.5m, lineItem.Quantity);
        Assert.Equal(8445, lineItem.Rate);
        Assert.Equal(budgetCategoryId, lineItem.BudgetCategoryId);
        Assert.Equal(LineItemValidationStatus.Valid, lineItem.ValidationStatus);
        Assert.Null(lineItem.ValidationMessage);
    }

    [Fact]
    public void Create_CalculatesAmountCorrectly()
    {
        var lineItem = InvoiceLineItem.Create(
            Guid.NewGuid(), "01_002_0107_1_1", "Desc",
            new DateOnly(2025, 7, 7), 3m, 8445);

        // 3 * 8445 = 25335 cents = $253.35
        Assert.Equal(25335, lineItem.Amount);
    }

    [Fact]
    public void Create_CalculatesDecimalQuantityAmount()
    {
        var lineItem = InvoiceLineItem.Create(
            Guid.NewGuid(), "01_002_0107_1_1", "Desc",
            new DateOnly(2025, 7, 7), 1.5m, 10000);

        // 1.5 * 10000 = 15000 cents = $150.00
        Assert.Equal(15000, lineItem.Amount);
    }

    [Fact]
    public void Create_WithoutBudgetCategory_SetsNull()
    {
        var lineItem = InvoiceLineItem.Create(
            Guid.NewGuid(), "01_002_0107_1_1", "Desc",
            new DateOnly(2025, 7, 7), 1m, 5000);

        Assert.Null(lineItem.BudgetCategoryId);
    }

    [Fact]
    public void UpdateValidation_SetsStatusAndMessage()
    {
        var lineItem = InvoiceLineItem.Create(
            Guid.NewGuid(), "01_002_0107_1_1", "Desc",
            new DateOnly(2025, 7, 7), 1m, 5000);

        lineItem.UpdateValidation(LineItemValidationStatus.Warning, "Rate exceeds price limit");

        Assert.Equal(LineItemValidationStatus.Warning, lineItem.ValidationStatus);
        Assert.Equal("Rate exceeds price limit", lineItem.ValidationMessage);
    }

    [Fact]
    public void UpdateValidation_ToValid_ClearsMessage()
    {
        var lineItem = InvoiceLineItem.Create(
            Guid.NewGuid(), "01_002_0107_1_1", "Desc",
            new DateOnly(2025, 7, 7), 1m, 5000);

        lineItem.UpdateValidation(LineItemValidationStatus.Warning, "Some warning");
        lineItem.UpdateValidation(LineItemValidationStatus.Valid, null);

        Assert.Equal(LineItemValidationStatus.Valid, lineItem.ValidationStatus);
        Assert.Null(lineItem.ValidationMessage);
    }
}
