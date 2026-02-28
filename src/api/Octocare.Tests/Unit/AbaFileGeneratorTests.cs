using Octocare.Application.Services;
using Octocare.Domain.Entities;

namespace Octocare.Tests.Unit;

public class AbaFileGeneratorTests
{
    private readonly AbaFileGenerator _generator = new();

    private static PaymentBatch CreateBatchWithItems()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST-001");

        // We need to add items AND set up provider bank details.
        // Since Provider has a private constructor, we'll use the Create factory and UpdateBankDetails.
        var provider1 = Provider.Create("Therapy Solutions", "12345678901", "info@therapy.com.au");
        provider1.UpdateBankDetails("032-001", "123456789", "THERAPY SOLUTIONS");

        var provider2 = Provider.Create("Care Plus", "98765432109", "hello@careplus.com.au");
        provider2.UpdateBankDetails("062-000", "987654321", "CARE PLUS PTY LTD");

        var item1 = PaymentItem.Create(batch.Id, provider1.Id, "Therapy Solutions", 250000, "inv1,inv2");
        var item2 = PaymentItem.Create(batch.Id, provider2.Id, "Care Plus", 175050, "inv3");

        // Use reflection to set the Provider navigation property for testing
        SetProvider(item1, provider1);
        SetProvider(item2, provider2);

        batch.AddItem(item1);
        batch.AddItem(item2);

        return batch;
    }

    private static void SetProvider(PaymentItem item, Provider provider)
    {
        var prop = typeof(PaymentItem).GetProperty("Provider");
        prop!.SetValue(item, provider);
    }

    [Fact]
    public void Generate_ReturnsNonEmptyString()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Generate_ContainsThreeRecordTypes()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Type 0 (header), 2 x Type 1 (detail), Type 7 (total) = 4 lines
        Assert.Equal(4, lines.Length);
        Assert.Equal('0', lines[0][0]); // Descriptive record
        Assert.Equal('1', lines[1][0]); // Detail record 1
        Assert.Equal('1', lines[2][0]); // Detail record 2
        Assert.Equal('7', lines[3][0]); // File total record
    }

    [Fact]
    public void Generate_AllRecordsAre120Characters()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            Assert.Equal(120, line.Length);
        }
    }

    [Fact]
    public void Generate_DescriptiveRecord_ContainsCompanyName()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var header = lines[0];
        Assert.StartsWith("0", header);
        Assert.Contains("OCTOCARE PTY LTD", header);
    }

    [Fact]
    public void Generate_DetailRecord_ContainsProviderBsb()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // First detail record should contain provider 1's BSB
        var detail1 = lines[1];
        Assert.Equal('1', detail1[0]);
        // BSB at positions 2-8 (index 1-7)
        Assert.Equal("032-001", detail1.Substring(1, 7));
    }

    [Fact]
    public void Generate_DetailRecord_ContainsAmountZeroPadded()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // First detail record amount = 250000 cents at positions 21-30 (index 20-29)
        var detail1 = lines[1];
        var amount = detail1.Substring(20, 10);
        Assert.Equal("0000250000", amount);
    }

    [Fact]
    public void Generate_DetailRecord_TransactionCode53()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Transaction code at positions 19-20 (index 18-19)
        var detail1 = lines[1];
        Assert.Equal("53", detail1.Substring(18, 2));
    }

    [Fact]
    public void Generate_FileTotalRecord_StartsWithType7()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var total = lines[^1];
        Assert.Equal('7', total[0]);
        Assert.Equal("999-999", total.Substring(1, 7));
    }

    [Fact]
    public void Generate_FileTotalRecord_ContainsCorrectTotals()
    {
        var batch = CreateBatchWithItems();

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        var total = lines[^1];
        // Net total at positions 21-30 (index 20-29): 250000 + 175050 = 425050
        var netTotal = total.Substring(20, 10);
        Assert.Equal("0000425050", netTotal);

        // Credit total at positions 31-40 (index 30-39)
        var creditTotal = total.Substring(30, 10);
        Assert.Equal("0000425050", creditTotal);

        // Debit total at positions 41-50 (index 40-49)
        var debitTotal = total.Substring(40, 10);
        Assert.Equal("0000000000", debitTotal);

        // Record count at positions 57-62 (index 56-61)
        var recordCount = total.Substring(56, 6);
        Assert.Equal("000002", recordCount);
    }

    [Fact]
    public void Generate_EmptyBatch_HasOnlyHeaderAndTotal()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-EMPTY");

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal(2, lines.Length);
        Assert.Equal('0', lines[0][0]);
        Assert.Equal('7', lines[1][0]);
    }

    [Fact]
    public void Generate_SkipsProvidersWithoutBankDetails()
    {
        var batch = PaymentBatch.Create(Guid.NewGuid(), "PAY-TEST");

        var providerWithBank = Provider.Create("Provider With Bank");
        providerWithBank.UpdateBankDetails("032-001", "123456789", "PROVIDER WITH BANK");

        var providerWithoutBank = Provider.Create("Provider Without Bank");
        // No bank details set

        var item1 = PaymentItem.Create(batch.Id, providerWithBank.Id, "Provider With Bank", 100000, "inv1");
        var item2 = PaymentItem.Create(batch.Id, providerWithoutBank.Id, "Provider Without Bank", 50000, "inv2");

        SetProvider(item1, providerWithBank);
        SetProvider(item2, providerWithoutBank);

        batch.AddItem(item1);
        batch.AddItem(item2);

        var result = _generator.Generate(batch, "032-000", "000000000", "OCTOCARE PTY LTD", "OCTOCARE PTY LTD");
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Header + 1 detail (only provider with bank) + Total = 3 lines
        Assert.Equal(3, lines.Length);
        Assert.Equal('1', lines[1][0]);
    }
}
