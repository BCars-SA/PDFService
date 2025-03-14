namespace API.Models.Pdf;

public abstract class PdfField
{
    public virtual string? Name { get; }
    public virtual object? Value { get; set; }
    public virtual List<string?>? AllValues { get; set; }
    public virtual string? Type { get; }
    public virtual int? Page { get; }
    public virtual bool? IsReadOnly {  get; }
    public virtual string? DisplayValue { get; }
    public virtual List<PdfField>? ChildFields { get; }
}
