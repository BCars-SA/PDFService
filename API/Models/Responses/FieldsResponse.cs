namespace API.Models.Responses;

public class FieldsResponse
{
    public List<Page>? Pages { get; set; }
    public List<string>? Fonts { get; set; }
    public int FieldsCount { get; set; }
    public List<Field>? Fields { get; set; }

    public class Field {
        public string? Name { get; set; }
        public object? Value { get; set; }
        public string? Type { get; set; }
        public int? Page { get; set; }
        public bool? IsReadOnly { get; set; }
        public List<string?>? ValueOptions { get; set; }
        public List<Field>? ChildFields { get; set; } 
    }

    public class Page
    {
        public int? Number { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
    }
}