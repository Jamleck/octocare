using System.Text;
using Octocare.Domain.Entities;

namespace Octocare.Application.Services;

/// <summary>
/// Generates ABA (Australian Banking Association) format files for direct entry payments.
/// Each record is exactly 120 characters wide, terminated with CRLF.
/// </summary>
public class AbaFileGenerator
{
    /// <summary>
    /// Generate an ABA file from a payment batch with provider bank details.
    /// </summary>
    /// <param name="batch">The payment batch containing items.</param>
    /// <param name="bankBsb">The originating bank's BSB (e.g., "032-000").</param>
    /// <param name="bankAccountNumber">The originating bank account number.</param>
    /// <param name="bankAccountName">The originating bank account name.</param>
    /// <param name="companyName">The company name for the descriptive record.</param>
    /// <param name="apcaId">The APCA user identification number (6 digits).</param>
    /// <param name="description">A description for the descriptive record.</param>
    /// <returns>ABA file content as a string.</returns>
    public string Generate(
        PaymentBatch batch,
        string bankBsb,
        string bankAccountNumber,
        string bankAccountName,
        string companyName,
        string apcaId = "000000",
        string description = "PAYMENTS")
    {
        var sb = new StringBuilder();

        // Type 0 — Descriptive Record
        sb.AppendLine(GenerateDescriptiveRecord(companyName, apcaId, description, bankBsb, bankAccountNumber, bankAccountName));

        // Type 1 — Detail Records
        long totalCredit = 0;
        int recordCount = 0;
        foreach (var item in batch.Items)
        {
            var provider = item.Provider;
            if (provider?.Bsb is null || provider?.AccountNumber is null)
                continue;

            sb.AppendLine(GenerateDetailRecord(
                provider.Bsb,
                provider.AccountNumber,
                provider.AccountName ?? provider.Name,
                item.Amount,
                bankBsb,
                bankAccountNumber,
                bankAccountName,
                item.ProviderName));

            totalCredit += item.Amount;
            recordCount++;
        }

        // Type 7 — File Total Record
        sb.AppendLine(GenerateFileTotalRecord(totalCredit, recordCount));

        return sb.ToString();
    }

    /// <summary>
    /// Type 0 record — descriptive record (header).
    /// </summary>
    private static string GenerateDescriptiveRecord(
        string companyName,
        string apcaId,
        string description,
        string bankBsb,
        string bankAccountNumber,
        string bankAccountName)
    {
        var record = new char[120];
        Array.Fill(record, ' ');

        // Position 1: Record type
        record[0] = '0';

        // Position 2-18: Blank (17 spaces)
        // Already filled with spaces

        // Position 19-20: Reel sequence number (always "01")
        "01".CopyTo(0, record, 18, 2);

        // Position 21-23: Bank/State/Branch of user's financial institution (first 3 chars of BSB)
        var bsbClean = bankBsb.Replace("-", "");
        PadRight(bsbClean, 3).CopyTo(0, record, 20, 3);

        // Position 24-30: Blank (7 spaces)
        // Already filled with spaces

        // Position 31-56: User preferred name (26 chars, left-aligned)
        PadRight(companyName, 26).CopyTo(0, record, 30, 26);

        // Position 57-62: User identification number (6 digits)
        PadRight(apcaId, 6).CopyTo(0, record, 56, 6);

        // Position 63-74: Description of entries (12 chars)
        PadRight(description, 12).CopyTo(0, record, 62, 12);

        // Position 75-80: Date to be processed (DDMMYY)
        var dateStr = DateTime.UtcNow.ToString("ddMMyy");
        dateStr.CopyTo(0, record, 74, 6);

        // Position 81-120: Blank (40 spaces)
        // Already filled with spaces

        return new string(record);
    }

    /// <summary>
    /// Type 1 record — detail record (one per payment).
    /// </summary>
    private static string GenerateDetailRecord(
        string payeeBsb,
        string payeeAccountNumber,
        string payeeAccountName,
        long amountInCents,
        string senderBsb,
        string senderAccountNumber,
        string senderAccountName,
        string lodgementReference)
    {
        var record = new char[120];
        Array.Fill(record, ' ');

        // Position 1: Record type
        record[0] = '1';

        // Position 2-8: BSB of payee (format: NNN-NNN)
        var bsbFormatted = FormatBsb(payeeBsb);
        bsbFormatted.CopyTo(0, record, 1, 7);

        // Position 9-17: Account number of payee (9 chars, right-aligned, blank-padded)
        PadLeft(payeeAccountNumber.Trim(), 9).CopyTo(0, record, 8, 9);

        // Position 18: Indicator (" " for new/varied, default space)
        record[17] = ' ';

        // Position 19-20: Transaction code ("53" = Pay)
        "53".CopyTo(0, record, 18, 2);

        // Position 21-30: Amount (10 digits, right-aligned, zero-padded)
        var amountStr = Math.Abs(amountInCents).ToString().PadLeft(10, '0');
        amountStr.CopyTo(0, record, 20, 10);

        // Position 31-62: Title of account to be credited (32 chars, left-aligned)
        PadRight(payeeAccountName, 32).CopyTo(0, record, 30, 32);

        // Position 63-80: Lodgement reference (18 chars, left-aligned)
        PadRight(lodgementReference, 18).CopyTo(0, record, 62, 18);

        // Position 81-87: Trace BSB (sender's BSB, format: NNN-NNN)
        var senderBsbFormatted = FormatBsb(senderBsb);
        senderBsbFormatted.CopyTo(0, record, 80, 7);

        // Position 88-96: Trace account number (sender's, 9 chars, right-aligned, blank-padded)
        PadLeft(senderAccountNumber.Trim(), 9).CopyTo(0, record, 87, 9);

        // Position 97-112: Name of remitter (16 chars, left-aligned)
        PadRight(senderAccountName, 16).CopyTo(0, record, 96, 16);

        // Position 113-120: Withholding tax amount (8 digits, zero-padded)
        "00000000".CopyTo(0, record, 112, 8);

        return new string(record);
    }

    /// <summary>
    /// Type 7 record — file total record (trailer).
    /// </summary>
    private static string GenerateFileTotalRecord(long totalCredit, int recordCount)
    {
        var record = new char[120];
        Array.Fill(record, ' ');

        // Position 1: Record type
        record[0] = '7';

        // Position 2-8: BSB format filler "999-999"
        "999-999".CopyTo(0, record, 1, 7);

        // Position 9-20: Blank (12 spaces)
        // Already filled with spaces

        // Position 21-30: Net total (10 digits, zero-padded) - equals credit minus debit
        var netTotal = Math.Abs(totalCredit).ToString().PadLeft(10, '0');
        netTotal.CopyTo(0, record, 20, 10);

        // Position 31-40: Credit total (10 digits, zero-padded)
        var creditTotal = Math.Abs(totalCredit).ToString().PadLeft(10, '0');
        creditTotal.CopyTo(0, record, 30, 10);

        // Position 41-50: Debit total (10 digits, zero-padded) - always zero for payments
        "0000000000".CopyTo(0, record, 40, 10);

        // Position 51-56: Blank (6 spaces)
        // Already filled with spaces

        // Position 57-62: Number of type 1 records (6 digits, zero-padded)
        var countStr = recordCount.ToString().PadLeft(6, '0');
        countStr.CopyTo(0, record, 56, 6);

        // Position 63-120: Blank (58 spaces)
        // Already filled with spaces

        return new string(record);
    }

    private static string FormatBsb(string bsb)
    {
        var clean = bsb.Replace("-", "").PadRight(6, '0');
        return $"{clean[..3]}-{clean[3..6]}";
    }

    private static string PadRight(string value, int length)
    {
        if (value.Length > length)
            return value[..length];
        return value.PadRight(length);
    }

    private static string PadLeft(string value, int length)
    {
        if (value.Length > length)
            return value[..length];
        return value.PadLeft(length);
    }
}
