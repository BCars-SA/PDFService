using iText.Forms.Fields;

namespace API.Models.Pdf.Fields;

public class TextField : FormField
{
    public TextField(PdfFormField field) : base(field)
    {
    }

    public override string Type => FieldTypes.Text.GetString();
}
