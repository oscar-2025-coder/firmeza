using Firmeza.Admin.Models;
using QuestPDF.Fluent;

namespace Firmeza.Admin.Services.Pdf;

public class ReceiptPdfService
{
    public byte[] GenerateReceiptPdf(Sale sale)
    {
        var document = new ReceiptDocument(sale);
        return document.GeneratePdf();
    }
}