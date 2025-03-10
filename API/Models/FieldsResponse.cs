public class FieldsResponse<TField>
{
    public int FieldsCount { get; set; }
    public List<TField>? Fields { get; set; }
}