using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Firmeza.Infrastructure.Entities;

namespace Firmeza.Infrastructure.Services;

public interface IPdfService
{
    byte[] GenerateSaleReceipt(Sale sale);
}

public class PdfService : IPdfService
{
    public byte[] GenerateSaleReceipt(Sale sale)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text("COMPROBANTE DE COMPRA")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(x =>
                    {
                        x.Spacing(20);

                        // Company Info
                        x.Item().Text("Firmeza S.A.S").FontSize(16).SemiBold();
                        x.Item().Text("Materiales de Construcción y Maquinaria").FontSize(12);

                        // Sale Info
                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        x.Item().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"ID de Venta: {sale.Id}").FontSize(10);
                                col.Item().Text($"Fecha: {sale.Date:dd/MM/yyyy HH:mm}").FontSize(10);
                            });
                            
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text($"Cliente: {sale.Customer?.FullName ?? "N/A"}").FontSize(10);
                                col.Item().Text($"Email: {sale.Customer?.Email ?? "N/A"}").FontSize(10);
                            });
                        });

                        x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        // Items Table
                        x.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            // Header
                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("Producto").SemiBold();
                                header.Cell().Element(CellStyle).Text("Cantidad").SemiBold();
                                header.Cell().Element(CellStyle).Text("Precio Unit.").SemiBold();
                                header.Cell().Element(CellStyle).Text("Total").SemiBold();

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Black).PaddingVertical(5);
                                }
                            });

                            // Items
                            foreach (var item in sale.Items)
                            {
                                table.Cell().Element(CellStyle).Text(item.Product?.Name ?? "N/A");
                                table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                                table.Cell().Element(CellStyle).AlignRight().Text($"${item.UnitPrice:N0}");
                                table.Cell().Element(CellStyle).AlignRight().Text($"${item.Amount:N0}");

                                static IContainer CellStyle(IContainer container)
                                {
                                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                                }
                            }
                        });

                        // Totals
                        x.Item().PaddingTop(20).Column(col =>
                        {
                            col.Item().AlignRight().Row(row =>
                            {
                                row.ConstantItem(150).Text("Subtotal:").SemiBold();
                                row.ConstantItem(100).AlignRight().Text($"${sale.Subtotal:N0}");
                            });

                            col.Item().AlignRight().Row(row =>
                            {
                                row.ConstantItem(150).Text("IVA (19%):").SemiBold();
                                row.ConstantItem(100).AlignRight().Text($"${sale.Tax:N0}");
                            });

                            col.Item().AlignRight().PaddingTop(10).Row(row =>
                            {
                                row.ConstantItem(150).Text("TOTAL:").FontSize(14).SemiBold();
                                row.ConstantItem(100).AlignRight().Text($"${sale.Total:N0}").FontSize(14).SemiBold().FontColor(Colors.Green.Medium);
                            });
                        });

                        // Footer
                        x.Item().PaddingTop(30).Column(col =>
                        {
                            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                            col.Item().PaddingTop(10).Text("Gracias por su compra").FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                        });
                    });

                page.Footer()
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(8).FontColor(Colors.Grey.Medium))
                    .Text($"Firmeza S.A.S - Generado el {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
        });

        return document.GeneratePdf();
    }
}
