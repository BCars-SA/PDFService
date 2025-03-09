using iText.Forms.Fields;

namespace API.Models.Pdf.Fields;

public static class FieldFactory
{
    private static Dictionary<Type, Type> TYPE_MAP_DICT = new()
    {
        { typeof(PdfTextFormField), typeof(TextField) },
        { typeof(PdfChoiceFormField), typeof(ChoiceField) }
    };

    public static IField Create(PdfFormField formField)
    {
        Type type = formField.GetType();

        if (TYPE_MAP_DICT.ContainsKey(type))
        {
            Type fieldType = TYPE_MAP_DICT[type];

            if (fieldType != null)
            {
                var field = Activator.CreateInstance(fieldType, formField);
                if (field != null)
                    return (IField)field;
            }
        }

        return new BaseField(formField);
    }
}
