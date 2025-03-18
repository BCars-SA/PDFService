using API.Converters;
using iText.Forms.Fields;
using System.Text.Json.Serialization;

namespace API.Services.ITextPdfService.Fields;

public class BaseFormField : AbstractFormField
{
    protected PdfFormField FormField { get => (PdfFormField)_field; }

    public BaseFormField(PdfFormField field) : base(field)
    {
    }

    [JsonConverter(typeof(FieldValueJsonConverter))]
    public override object? Value
    {
        get
        {
            var fieldValue = FormField.GetValueAsString();
            return string.IsNullOrEmpty(fieldValue) ? null : fieldValue;
        }
        set
        {
            FormField.SetValue(value?.ToString());
        }
    }

    public override string? DisplayValue
    {
        get
        {
            var displayValue = FormField.GetDisplayValue();
            return string.IsNullOrEmpty(displayValue) || displayValue == FormField.GetValueAsString() ? null : displayValue;
        }
    }

    public override string Type => FieldTypes.Undefined.GetString();

    public override bool? IsReadOnly => FormField.IsReadOnly() ? true : null;
}