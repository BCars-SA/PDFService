using iText.Forms.Fields;

namespace API.Services.ITextPdfService.Fields;

public class ButtonField : BaseFormField
{
    PdfButtonFormField PdfButtonField { get => (PdfButtonFormField)_field; }

    public ButtonField(PdfFormField field) : base(field)
    {
    }
}
