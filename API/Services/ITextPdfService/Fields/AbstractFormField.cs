using API.Models.Pdf;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;

namespace API.Services.ITextPdfService.Fields;

public class AbstractFormField : PdfField
{
    protected AbstractPdfFormField _field;
    protected PdfDocument _document;

    public AbstractFormField(AbstractPdfFormField field)
    {
        _field = field;
        _document = _field.GetDocument();
    }

    public override string? Name => _field.GetFieldName()?.ToString();

    public override int? Page
    {
        get
        {
            PdfDictionary fieldObject = _field.GetPdfObject();

            PdfDictionary pageDic = fieldObject.GetAsDictionary(PdfName.P);
            if (pageDic != null)
            {
                return _document.GetPageNumber(pageDic);
            }

            int pageCount = _document.GetNumberOfPages();
            for (int i = 1; i <= pageCount; i++)
            {
                PdfPage page = _document.GetPage(i);

                if (!page.IsFlushed())
                {
                    PdfAnnotation annotation = PdfAnnotation.MakeAnnotation(fieldObject);
                    if (annotation != null && page.ContainsAnnotation(annotation))
                    {
                        return i;
                    }
                }
            }

            return null;
        }
    }
}
