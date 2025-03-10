using API.Controllers;
using API.Converters;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using System.Text.Json.Serialization;

namespace API.Models.Pdf.Fields;

public class FormField
{
    protected PdfFormField _field;
    protected PdfDocument _document;

    public FormField(PdfFormField field)
    {
        _field = field;
        _document = _field.GetDocument();
    }

    public string Name => _field.GetFieldName().ToString();

    [JsonConverter(typeof(FieldValueJsonConverter))]
    public virtual object? Value
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

    public virtual string? DisplayValue
    {
        get
        {
            var displayValue = _field.GetDisplayValue();
            return string.IsNullOrEmpty(displayValue) || displayValue == _field.GetValueAsString() ? null : displayValue;
        }
    }

    public virtual string Type => FieldTypes.Undefined.GetString();

    public bool? IsReadOnly => _field.IsReadOnly() ? true : null;

    public int? Page
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