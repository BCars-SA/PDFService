using API.Models.Pdf;
using API.Models.Requests;

namespace API.Services;

public interface IPdfService
{
    (List<PdfField> fields, List<PdfPageInfo> pages, List<string> fonts) ReadFields(IFormFile pdfFile);
    byte[] Fill(IFormFile pdfFile, List<FillRequest.Field> fields);
}
