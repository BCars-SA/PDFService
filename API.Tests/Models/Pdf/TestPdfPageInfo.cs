using API.Models.Pdf;

namespace API.Tests.Models.Pdf
{
    public class TestPdfPageInfo : PdfPageInfo
    {

        public TestPdfPageInfo(int number, float width, float height) 
        {
            Number = number;
            Width = width;
            Height = height;
        }

        public override int Number { get; }
        public override float? Width { get; }
        public override float? Height { get; }
    }
}
