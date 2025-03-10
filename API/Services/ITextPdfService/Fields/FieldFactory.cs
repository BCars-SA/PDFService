using API.Models.Pdf;
using iText.Forms.Fields;

namespace API.Services.ITextPdfService.Fields;

public static class FieldFactory
{
    private static Dictionary<Type, Type> TYPE_MAP_DICT = new()
    {
        { typeof(PdfTextFormField), typeof(TextField) },
        { typeof(PdfChoiceFormField), typeof(ChoiceField) },
        { typeof(PdfButtonFormField), typeof(ButtonField) }
    };

    public static PdfField Create(PdfFormField formField)
    {
        Type type = formField.GetType();

        if (TYPE_MAP_DICT.ContainsKey(type))
        {
            Type fieldType = TYPE_MAP_DICT[type];

            if (fieldType != null)
            {
                var field = Activator.CreateInstance(fieldType, formField);
                if (field != null)
                    return (PdfField)field;
            }
        }

        return new BaseFormField(formField);
    }
}
