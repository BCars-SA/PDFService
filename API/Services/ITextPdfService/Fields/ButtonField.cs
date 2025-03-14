using API.Models.Pdf;
using iText.Forms.Fields;

namespace API.Services.ITextPdfService.Fields;

public class ButtonField : BaseFormField
{
    PdfButtonFormField PdfButtonField { get => (PdfButtonFormField)_field; }

    public ButtonField(PdfFormField field) : base(field)
    {
    }

    public override object? Value 
    { 
        get => base.Value; 
        set
        {
            switch (value)
            {
                case null:

                    if (IsCheckBox)
                        throw new InvalidOperationException($"The value 'null' cannot be set for the checkbox '{Name}'.");

                    base.Value = value;
                    break;

                case string stringValue:

                    var allvalues = AllValues;
                    if (allvalues == null)
                        throw new InvalidOperationException($"The value '{stringValue}' cannot be set for the '{Name}' field.");

                    var valueToSet = allvalues.Find(s => stringValue.Equals(s, StringComparison.OrdinalIgnoreCase));
                    if (valueToSet == null)
                        throw new InvalidOperationException($"The value '{stringValue}' cannot be set for the '{Name}' field.");

                    base.Value = valueToSet;
                    break;

                default:

                    throw new ArgumentException($"An attempt to assign a value of an unsupported type '{value.GetType().FullName}' to the field '{Name}'");
            }
        }
    }

    public override string Type
    {
        get
        {
            return  IsPushButton ? FieldTypes.PushButton.GetString()
                : IsRadioGroup ? FieldTypes.RadioButtonGroup.GetString()
                : FieldTypes.CheckBox.GetString();
        }
    }

    private bool IsRadioGroup
    {
        get
        {
            var kids = PdfButtonField.GetKids();
            return kids == null ? false : kids.Count() > 1;
        }
    }

    private bool IsCheckBox
    {
        get => !IsPushButton && !IsRadioGroup;
    }

    private bool IsPushButton
    {
        get => PdfButtonField.IsPushButton();
    }

    public override List<string?>? AllValues
    {
        get => PdfButtonField.GetAppearanceStates()?.ToList();
    }

    public override List<PdfField>? ChildFields
    {
        get
        {
            var childFields = PdfButtonField.GetChildFields();

            if (childFields == null || childFields.Count == 0)
                return null;

            return childFields.Select(cf => new ButtonChildField(cf)).Cast<PdfField>().ToList();
        }
    }
}
