using API.Models.Pdf;
using iText.Kernel.Pdf;

namespace API.Services.ITextPdfService;

public class PageInfo : PdfPageInfo
{
    protected PdfPage _page;

    public PageInfo(PdfPage page)
    {
        _page = page;
    }

    public override int Number => _page.GetDocument().GetPageNumber(_page);
    public override float? Width => _page.GetPageSize().GetWidth();
    public override float? Height => _page.GetPageSize().GetHeight();
}
