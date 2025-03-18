namespace API.Services.ITextPdfService.Fields;

public enum FieldTypes
{
    [StringValue("text")]
    Text,

    [StringValue("combobox")]
    ComboBox,

    [StringValue("listbox")]
    ListBox,

    [StringValue("radiogroup")]
    RadioButtonGroup,

    [StringValue("checkbox")]
    CheckBox,

    [StringValue("pushbutton")]
    PushButton,

    [StringValue("undefined")]
    Undefined
}

public class StringValueAttribute : Attribute
{
    public string StringValue { get; protected set; }

    public StringValueAttribute(string value)
    {
        StringValue = value;
    }
}

public static class EnumExtensions
{
    public static string GetString(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());

        StringValueAttribute? attribute = null;
        if (fieldInfo != null)
            attribute = Attribute.GetCustomAttribute(fieldInfo, typeof(StringValueAttribute)) as StringValueAttribute;

        return attribute == null ? value.ToString() : attribute.StringValue;
    }

    public static T GetEnumFromString<T>(string value) where T : Enum
    {
        var type = typeof(T);

        foreach (var field in type.GetFields())
        {
            var attribute = Attribute.GetCustomAttribute(field, typeof(StringValueAttribute)) as StringValueAttribute;
            if (attribute != null && attribute.StringValue.Equals(value, StringComparison.OrdinalIgnoreCase))
            {
                var valueObject = field.GetValue(null);

                if (valueObject != null)
                    return (T)valueObject;
            }
        }

        throw new ArgumentException($"Undefined value: {value}");
    }
}
