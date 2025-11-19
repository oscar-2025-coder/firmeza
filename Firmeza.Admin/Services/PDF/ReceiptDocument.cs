using Firmeza.Admin.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Firmeza.Admin.Services.Pdf;

public class ReceiptDocument : IDocument
{
    public Sale Sale { get; }

    public ReceiptDocument(Sale sale)
    {
        Sale = sale;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.DefaultTextStyle(x => x.FontSize(12));

            // --------- Header ----------
            page.Header()
                .AlignCenter()
                .Text("Sale Receipt")
                .SemiBold()
                .FontSize(18);

            // --------- Content ----------
            page.Content().Column(column =>
            {
                column.Spacing(5);

                // Customer and sale info
                column.Item().Text(text =>
                {
                    text.Span("Customer: ").SemiBold();
                    text.Span(Sale.Customer.FullName);
                });

                column.Item().Text(text =>
                {
                    text.Span("Document: ").SemiBold();
                    text.Span(Sale.Customer.DocumentNumber);
                });

                column.Item().Text(text =>
                {
                    text.Span("Email: ").SemiBold();
                    text.Span(Sale.Customer.Email);
                });

                column.Item().Text(text =>
                {
                    text.Span("Phone: ").SemiBold();
                    text.Span(Sale.Customer.PhoneNumber);
                });

                column.Item().Text(text =>
                {
                    text.Span("Sale ID: ").SemiBold();
                    text.Span(Sale.Id.ToString());
                });

                column.Item().Text(text =>
                {
                    text.Span("Date: ").SemiBold();
                    text.Span(Sale.Date.ToString("yyyy-MM-dd HH:mm"));
                });

                if (!string.IsNullOrWhiteSpace(Sale.Notes))
                {
                    column.Item().Text(text =>
                    {
                        text.Span("Notes: ").SemiBold();
                        text.Span(Sale.Notes!);
                    });
                }

                // Espacio antes de la tabla
                column.Item().Text(string.Empty);

                // --------- Items table ----------
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();      // Product
                        columns.RelativeColumn(0.5f);  // Qty
                        columns.RelativeColumn(0.8f);  // Unit price
                        columns.RelativeColumn(0.8f);  // Amount
                    });

                    // Header row
                    table.Header(header =>
                    {
                        header.Cell().Text("Product").SemiBold();
                        header.Cell().Text("Qty").SemiBold();
                        header.Cell().Text("Unit Price").SemiBold();
                        header.Cell().Text("Amount").SemiBold();
                    });

                    // Data rows
                    foreach (var item in Sale.Items)
                    {
                        table.Cell().Text(item.Product.Name);
                        table.Cell().Text(item.Quantity.ToString());
                        table.Cell().Text($"${item.UnitPrice:F2}");
                        table.Cell().Text($"${item.Amount:F2}");
                    }
                });

                // Espacio antes de totales
                column.Item().Text(string.Empty);

                // --------- Totals ----------
                column.Item().AlignRight().Column(totals =>
                {
                    totals.Item().Text(text =>
                    {
                        text.Span("Subtotal: ").SemiBold();
                        text.Span($"${Sale.Subtotal:F2}");
                    });

                    totals.Item().Text(text =>
                    {
                        text.Span("Tax: ").SemiBold();
                        text.Span($"${Sale.Tax:F2}");
                    });

                    totals.Item().Text(text =>
                    {
                        text.Span("Total: ").SemiBold();
                        text.Span($"${Sale.Total:F2}");
                    });
                });
            });

            // --------- Footer ----------
            page.Footer()
                .AlignCenter()
                .Text("Thank you for your purchase!");
        });
    }
}
