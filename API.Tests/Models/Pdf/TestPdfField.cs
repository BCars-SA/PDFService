using API.Models.Pdf;

namespace API.Tests.Models.Pdf
{
    public class TestPdfField : PdfField
    {
        public TestPdfField(string name, string type, int page, bool isReadOnly, string displayValue)
        {
            Name = name;            
            Type = type;
            Page = page;
            IsReadOnly = isReadOnly;
            DisplayValue = displayValue;
        }

        override public string? Name { get; }
        override public object? Value { get; set; }
        override public string? Type { get; }
        override public int? Page { get; }
        override public bool? IsReadOnly { get; }
        override public string? DisplayValue { get; }
    }
}