namespace API.Models.Pdf;

public abstract class PdfPageInfo
{
    public virtual int Number { get; }
    public virtual float? Width { get; }
    public virtual float? Height { get; }
}
