using System.Globalization;
using System.Text;
using Octocare.Domain.Entities;
using Octocare.Domain.ValueObjects;

namespace Octocare.Application.Services;

/// <summary>
/// Generates NDIA bulk payment request CSV files for claim submissions.
/// </summary>
public class NdiaCsvExporter
{
    private static readonly string[] Headers =
    [
        "RegistrationNumber",
        "NDISNumber",
        "SupportItemNumber",
        "DateOfSupport",
        "Quantity",
        "Hours",
        "UnitPrice",
        "TotalCost",
        "ClaimReference",
        "CancellationReason",
        "ABN"
    ];

    /// <summary>
    /// Generates the NDIA bulk payment CSV from a claim with fully loaded navigation properties.
    /// </summary>
    public byte[] Generate(Claim claim, string organisationAbn)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", Headers));

        foreach (var lineItem in claim.LineItems)
        {
            var invoiceLineItem = lineItem.InvoiceLineItem;
            var invoice = invoiceLineItem.Invoice;
            var participant = invoice.Participant;
            var provider = invoice.Provider;

            var registrationNumber = provider.Abn ?? string.Empty;
            var ndisNumber = participant.NdisNumber;
            var supportItemNumber = invoiceLineItem.SupportItemNumber;
            var dateOfSupport = invoiceLineItem.ServiceDate.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            var quantity = invoiceLineItem.Quantity.ToString("G", CultureInfo.InvariantCulture);
            var hours = invoiceLineItem.Quantity.ToString("G", CultureInfo.InvariantCulture);
            var unitPrice = new Money(invoiceLineItem.Rate).ToDollars().ToString("F2", CultureInfo.InvariantCulture);
            var totalCost = new Money(invoiceLineItem.Amount).ToDollars().ToString("F2", CultureInfo.InvariantCulture);
            var claimReference = claim.BatchNumber;
            var cancellationReason = string.Empty;
            var abn = organisationAbn;

            sb.AppendLine(string.Join(",",
                CsvEscape(registrationNumber),
                CsvEscape(ndisNumber),
                CsvEscape(supportItemNumber),
                CsvEscape(dateOfSupport),
                CsvEscape(quantity),
                CsvEscape(hours),
                CsvEscape(unitPrice),
                CsvEscape(totalCost),
                CsvEscape(claimReference),
                CsvEscape(cancellationReason),
                CsvEscape(abn)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string CsvEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
