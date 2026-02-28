using Octocare.Application.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Octocare.Application.Services;

public class StatementPdfGenerator
{
    public byte[] Generate(StatementData data)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, data));
                page.Content().Element(c => ComposeContent(c, data));
                page.Footer().Element(c => ComposeFooter(c, data));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, StatementData data)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(data.OrganisationName)
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    if (!string.IsNullOrWhiteSpace(data.OrganisationEmail))
                        col.Item().Text(data.OrganisationEmail).FontSize(9).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrWhiteSpace(data.OrganisationPhone))
                        col.Item().Text(data.OrganisationPhone).FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Text("Participant Statement")
                .FontSize(16).SemiBold().FontColor(Colors.Grey.Darken3);

            column.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Participant: {data.ParticipantName}").FontSize(10);
                    col.Item().Text($"NDIS Number: {data.NdisNumber}").FontSize(10);
                });
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Plan: {data.PlanNumber}").FontSize(10);
                    col.Item().Text($"Plan Period: {data.PlanStart:dd/MM/yyyy} - {data.PlanEnd:dd/MM/yyyy}").FontSize(10);
                });
            });

            column.Item().PaddingTop(4).Text(
                $"Statement Period: {data.PeriodStart:dd/MM/yyyy} - {data.PeriodEnd:dd/MM/yyyy}")
                .FontSize(10).SemiBold();

            column.Item().PaddingVertical(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private static void ComposeContent(IContainer container, StatementData data)
    {
        container.Column(column =>
        {
            // Budget Summary
            column.Item().PaddingBottom(4).Text("Budget Summary")
                .FontSize(13).SemiBold().FontColor(Colors.Blue.Darken3);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Category
                    columns.RelativeColumn(2); // Purpose
                    columns.RelativeColumn(1); // Allocated
                    columns.RelativeColumn(1); // Spent
                    columns.RelativeColumn(1); // Available
                    columns.RelativeColumn(1); // Utilisation
                });

                // Header row
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Category").FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Purpose").FontColor(Colors.White).FontSize(9).SemiBold();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Allocated").FontColor(Colors.White).FontSize(9).SemiBold().AlignRight();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Spent").FontColor(Colors.White).FontSize(9).SemiBold().AlignRight();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Available").FontColor(Colors.White).FontSize(9).SemiBold().AlignRight();
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                        .Text("Utilisation").FontColor(Colors.White).FontSize(9).SemiBold().AlignRight();
                });

                var isAlternate = false;
                foreach (var line in data.BudgetLines)
                {
                    var bg = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                    table.Cell().Background(bg).Padding(5).Text(line.Category).FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text(line.Purpose).FontSize(9);
                    table.Cell().Background(bg).Padding(5).Text($"${line.Allocated:N2}").FontSize(9).AlignRight();
                    table.Cell().Background(bg).Padding(5).Text($"${line.Spent:N2}").FontSize(9).AlignRight();
                    table.Cell().Background(bg).Padding(5).Text($"${line.Available:N2}").FontSize(9).AlignRight();
                    table.Cell().Background(bg).Padding(5).Text($"{line.UtilisationPercent:N1}%").FontSize(9).AlignRight();

                    isAlternate = !isAlternate;
                }

                // Totals row
                table.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text("Total").FontSize(9).Bold();
                table.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text("").FontSize(9);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text($"${data.TotalAllocated:N2}").FontSize(9).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text($"${data.TotalSpent:N2}").FontSize(9).Bold().AlignRight();
                table.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text($"${data.TotalAvailable:N2}").FontSize(9).Bold().AlignRight();

                var totalUtilisation = data.TotalAllocated > 0
                    ? Math.Round(data.TotalSpent / data.TotalAllocated * 100, 1)
                    : 0m;
                table.Cell().Background(Colors.Grey.Lighten2).Padding(5)
                    .Text($"{totalUtilisation:N1}%").FontSize(9).Bold().AlignRight();
            });

            // Recent Invoices
            column.Item().PaddingTop(16).PaddingBottom(4).Text("Recent Invoices")
                .FontSize(13).SemiBold().FontColor(Colors.Blue.Darken3);

            if (data.RecentInvoices.Count == 0)
            {
                column.Item().Padding(10).Text("No invoices found for this period.")
                    .FontSize(9).FontColor(Colors.Grey.Darken1).Italic();
            }
            else
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1); // Date
                        columns.RelativeColumn(2); // Provider
                        columns.RelativeColumn(3); // Description
                        columns.RelativeColumn(1); // Amount
                        columns.RelativeColumn(1); // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Date").FontColor(Colors.White).FontSize(9).SemiBold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Provider").FontColor(Colors.White).FontSize(9).SemiBold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Description").FontColor(Colors.White).FontSize(9).SemiBold();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Amount").FontColor(Colors.White).FontSize(9).SemiBold().AlignRight();
                        header.Cell().Background(Colors.Blue.Darken3).Padding(5)
                            .Text("Status").FontColor(Colors.White).FontSize(9).SemiBold();
                    });

                    var isAlternate = false;
                    foreach (var invoice in data.RecentInvoices)
                    {
                        var bg = isAlternate ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Background(bg).Padding(5).Text(invoice.ServiceDate.ToString("dd/MM/yyyy")).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(invoice.ProviderName).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text(invoice.Description).FontSize(9);
                        table.Cell().Background(bg).Padding(5).Text($"${invoice.Amount:N2}").FontSize(9).AlignRight();
                        table.Cell().Background(bg).Padding(5).Text(invoice.Status).FontSize(9);

                        isAlternate = !isAlternate;
                    }
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, StatementData data)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(data.OrganisationName).FontSize(8).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrWhiteSpace(data.OrganisationEmail))
                        col.Item().Text(data.OrganisationEmail).FontSize(8).FontColor(Colors.Grey.Darken1);
                    if (!string.IsNullOrWhiteSpace(data.OrganisationPhone))
                        col.Item().Text(data.OrganisationPhone).FontSize(8).FontColor(Colors.Grey.Darken1);
                });
                row.RelativeItem().AlignRight().Column(col =>
                {
                    col.Item().AlignRight().Text($"Generated on {DateTimeOffset.UtcNow:dd/MM/yyyy}")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignRight().Text(text =>
                    {
                        text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                        text.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                    });
                });
            });
        });
    }
}
