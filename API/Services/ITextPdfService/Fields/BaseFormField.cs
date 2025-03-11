using API.Converters;
using API.Models.Pdf;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using System.Text.Json.Serialization;

namespace API.Services.ITextPdfService.Fields;

public class BaseFormField : PdfField
{
    protected PdfFormField _field;
    protected PdfDocument _document;

    public BaseFormField(PdfFormField field)
    {
        _field = field;
        _document = _field.GetDocument();
    }

    public override string Name => _field.GetFieldName().ToString();

    [JsonConverter(typeof(FieldValueJsonConverter))]
    public override object? Value
    {
        get
        {
            var fieldValue = _field.GetValueAsString();
            return string.IsNullOrEmpty(fieldValue) ? null : fieldValue;
        }
        set
        {
            _field.SetValue(value?.ToString());
        }
    }

    public override string? DisplayValue
    {
        get
        {
            var displayValue = _field.GetDisplayValue();
            return string.IsNullOrEmpty(displayValue) || displayValue == _field.GetValueAsString() ? null : displayValue;
        }
    }

    public override string Type => FieldTypes.Undefined.GetString();

    public override bool? IsReadOnly => _field.IsReadOnly() ? true : null;

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