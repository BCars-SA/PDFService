using iText.Forms.Fields;
using iText.Kernel.Pdf;

namespace API.Services.ITextPdfService.Fields;

public class ButtonChildField : AbstractFormField
{
    public ButtonChildField(AbstractPdfFormField field) : base(field)
    {
    }

    public override object? Value 
    {
        get => _field.GetPdfObject().GetAsName(PdfName.AS)?.GetValue(); 
        set => throw new InvalidOperationException("It is not possible to set the value directly for the button in the radio button group"); 
    }

    public override List<string?>? ValueOptions
    {
        get => _field.GetAppearanceStates()?.ToList();
    }
}
