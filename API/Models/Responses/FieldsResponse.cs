namespace API.Models.Responses;

public class FieldsResponse
{
    public int FieldsCount { get; set; }
    public List<Field>? Fields { get; set; }

    public class Field {
        public string? Name { get; set; }
        public object? Value { get; set; }

        public string? Type { get; set; }
        public int? Page { get; set; }
    }
}