using iText.Forms.Fields;

namespace API.Services.ITextPdfService.Fields;

public class TextField : BaseFormField
{
    public TextField(PdfFormField field) : base(field)
    {
    }

    public override string Type => FieldTypes.Text.GetString();
}
