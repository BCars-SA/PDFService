using iText.Forms.Fields;
using iText.Kernel.Pdf;

namespace API.Services.ITextPdfService.Fields;

public class ChoiceField : BaseFormField
{
    PdfChoiceFormField PdfChoiceField { get => (PdfChoiceFormField)_field; }

    public ChoiceField(PdfChoiceFormField field) : base(field)
    {
    }

    public override object? Value
    {
        get
        {
            var indexes = PdfChoiceField.GetIndices().ToIntArray();

            if (indexes.Length > 0)
            {
                var selectedOptions = GetFieldOptionsToUnicodeNames().Where((o, i) => indexes.Contains(i)).ToList();
                var selectedOptionsCount = selectedOptions.Count();

                return selectedOptionsCount == 1 ? selectedOptions[0] : selectedOptions;
            }
            else
            {
                var value = PdfChoiceField.GetValue();

                if (value is PdfArray)
                    return ((PdfArray)value).ToList<PdfObject>().ConvertAll(o => ((PdfString)o).ToUnicodeString());

                if (value is PdfString)
                    return ((PdfString)value).ToUnicodeString();
            }

            return null;
        }

        set
        {
            List<string?> options = GetFieldOptionsToUnicodeNames();

            switch (value)
            {
                case null:

                    if (IsCombo && !IsEdit)
                        throw new InvalidOperationException($"The value cannot be empty. The field '{Name}' is a combobox and cannot be edited.");

                    PdfChoiceField.SetListSelected(new int[] { });
                    break;

                case int index:

                    if (index == -1)
                        goto case null;

                    if (index < -1 || index >= options.Count)
                        throw new IndexOutOfRangeException($"Index is out of range. Must be non-negative and less than {options.Count}. The field name: '{Name}'.");

                    PdfChoiceField.SetListSelected(new int[] { index });
                    break;

                case List<int> indexesToSet: //Setting values by indexes of the list

                    if (indexesToSet.Count > 0)
                    {
                        indexesToSet.Sort();

                        int minIndex = indexesToSet[0];
                        int maxIndex = indexesToSet[indexesToSet.Count - 1];

                        if (minIndex != maxIndex && !IsMultiSelect)
                            throw new InvalidOperationException($"An attempt to set multiple indexes for the '{Name}' field when at most one item should be selected");

                        if (minIndex < 0 || maxIndex >= options.Count)
                            throw new IndexOutOfRangeException($"Index is out of range. Must be non-negative and less than {options.Count}. The field name: '{Name}'.");
                    }
                    else if (IsCombo && !IsEdit)
                    {
                        throw new InvalidOperationException($"The value cannot be empty. The field '{Name}' is a combobox and cannot be edited.");
                    }

                    PdfChoiceField.SetListSelected(indexesToSet.ToArray());
                    break;

                case string stringValue:

                    var option = options.Find(s => stringValue.Equals(s, StringComparison.OrdinalIgnoreCase));

                    if (option == null && (!IsCombo || !IsEdit))
                        throw new InvalidOperationException($"The value '{stringValue}' cannot be set for the '{Name}' field. The list does not contain this value and cannot be edited.");

                    base.Value = option ?? stringValue;
                    break;

                case List<string> stringList:

                    stringList.RemoveAll(s => s == null);

                    if (stringList.Count > 0)
                    {
                        if (!IsMultiSelect && stringList.Count > 1)
                            throw new InvalidOperationException($"An attempt to set multiple values for the '{Name}' field when at most one item should be selected");

                        var optionsToSet = new List<string>();

                        foreach (var stringValue in stringList)
                        {
                            var optionToSet = options.Find(s => stringValue.Equals(s, StringComparison.OrdinalIgnoreCase));
                            if (optionToSet == null)
                            {
                                if (!IsCombo || !IsEdit)
                                    throw new InvalidOperationException($"The value '{stringValue}' cannot be set for the '{Name}' field. The list does not contain this value and cannot be edited.");

                                if (!optionsToSet.Contains(stringValue))
                                    optionsToSet.Add(stringValue);
                            }
                            else if (!optionsToSet.Contains(optionToSet))
                                optionsToSet.Add(optionToSet);
                        }

                        PdfChoiceField.SetListSelected(optionsToSet.ToArray());
                    }
                    else
                    {
                        if (IsCombo)
                        {
                            if (!IsEdit)
                                throw new InvalidOperationException($"The value cannot be empty. The field '{Name}' is a combobox and cannot be edited.");

                            base.Value = "";
                        }
                        else
                        {
                            PdfChoiceField.SetListSelected(new int[] { });
                        }
                    }

                    break;

                default:
                    throw new ArgumentException($"An attempt to assign a value of an unsupported type '{value.GetType().FullName}' to the field '{Name}'");
            }
        }
    }

    bool IsCombo
    {
        get => PdfChoiceField.IsCombo();
    }

    bool IsEdit
    {
        get => PdfChoiceField.IsEdit();
    }

    bool IsMultiSelect
    {
        get => PdfChoiceField.IsMultiSelect();
    }

    public override string Type
    {
        get
        {
            return PdfChoiceField.IsCombo() ? FieldTypes.ComboBox.GetString() : FieldTypes.ListBox.GetString();
        }
    }

    private List<string?> GetFieldOptionsToUnicodeNames()
    {
        PdfArray options = PdfChoiceField.GetOptions();
        var optionsToUnicodeNames = new List<string?>(options.Size());
        for (int index = 0; index < options.Size(); index++)
        {
            PdfObject option = options.Get(index);
            PdfString? value = null;
            if (option.IsString())
            {
                value = (PdfString)option;
            }
            else
            {
                if (option.IsArray() && ((PdfArray)option).Size() > 1)
                {
                    value = (PdfString)((PdfArray)option).Get(1);
                }
            }

            optionsToUnicodeNames.Add(value?.ToUnicodeString());
        }

        return optionsToUnicodeNames;
    }
}
