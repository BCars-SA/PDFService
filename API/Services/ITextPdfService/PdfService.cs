using API.Models.Pdf;
using API.Models.Requests;
using API.Services.ITextPdfService.Fields;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Image;
using iText.IO.Source;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Action;
using iText.Layout;
using iText.Layout.Element;
using NLog;
using System.Drawing;
using System.Text.RegularExpressions;
using static API.Models.Requests.FillRequest;
using iTextProperties = iText.Layout.Properties;

namespace API.Services.ITextPdfService;

public class PdfService : IPdfService
{
    private IConfiguration? _configuration;
    private Logger _logger;

    public PdfService(IConfiguration? configuration)
    {
        _configuration = configuration;
        _logger = NLog.LogManager.GetCurrentClassLogger();
    }

    public (List<PdfField> fields, List<PdfPageInfo> pages, List<string> fonts) ReadFields(IFormFile pdfFile)
    {
        PdfDocument pdfDocument = OpenPdfDocument(pdfFile, PdfFileOpenMode.Read, out _);
        var fontsHelper = new FontsHelper(pdfDocument, _configuration, _logger);

        return (ReadAllFormFields(pdfDocument, false), ReadAllPagesInfo(pdfDocument), fontsHelper.AllAvailableFontsKeys);
    }

    public byte[] Fill(IFormFile pdfFile, List<FillRequest.Field> fields)
    {
        PdfDocument pdfDocument = OpenPdfDocument(pdfFile, PdfFileOpenMode.ReadWrite, out var outputStream);
        Document document = new Document(pdfDocument);

        var fontsHelper = new FontsHelper(pdfDocument, _configuration, _logger);
        var colorConverter = new ColorConverter();

        List<PdfField> formFields = ReadAllFormFields(pdfDocument, true);
        
        // field.Name is not null here because of the ReadAllFormFields method
        var fieldsDictionary = formFields.ToDictionary(f => f.Name!.ToLower(), f => f);

        foreach (var field in fields)
        {
            var fieldName = field.Name?.ToLower();
            if (fieldName != null)
            {
                if (!fieldsDictionary.ContainsKey(fieldName))
                    throw new ArgumentException($"The document field with the name '{field.Name}' not found.");

                var pdfField = fieldsDictionary[fieldName];
                pdfField.Value = field.Value;                
            }
            else
            {
                if (field.Value != null && field.Value is string)
                {
                    //Page data
                    var pageNum = field.Page ?? 1;
                    var pagesCount = pdfDocument.GetNumberOfPages();
                    if (pageNum < 1 || pageNum > pagesCount)
                        throw new ArgumentException($"Incorrect page number: '{pageNum}'. The expected value is in the range [1, {pagesCount}].");
                    
                    PdfPage page = pdfDocument.GetPage(pageNum);
                    var pageSize = page.GetPageSize();
                    var pageWidth = pageSize.GetWidth();
                    var pageHeight = pageSize.GetHeight();
                    //

                    var stringValue = field.Value as string;

                    if (IsBase64String(stringValue!, out Span<byte> buffer)) //Image
                    {
                        ImageData data = ImageDataFactory.Create(buffer.ToArray());
                        var image = new Image(data);

                        var imageRealWidth = image.GetImageWidth();
                        var imageRealHeight = image.GetImageHeight();

                        var imageWidth = field.Width ?? imageRealWidth;
                        var imageHeight = field.Height ?? imageRealHeight;

                        var scaled = false;

                        if (field.Scale != null)
                        {
                            var scale = field.Scale.Value;
                            if (scale <= 0)
                                throw new ArgumentException($"Invalid image scale value: '{scale}'. The scale must be greater than 0.");

                            image.Scale(scale, scale);
                            scaled = true;
                        }
                        else if (imageRealWidth != imageWidth || imageRealHeight != imageHeight)
                        {
                            image.ScaleToFit(imageWidth, imageHeight);
                            scaled = true;
                        }

                        if (scaled)
                        {
                            imageWidth = image.GetImageScaledWidth();
                            imageHeight = image.GetImageScaledHeight();
                        }

                        if (imageWidth <= 0 || imageWidth > pageWidth)
                            throw new ArgumentException($"Invalid image width value: '{imageWidth}'. The width of the image must be greater than 0 and less than the width of the page: '{pageWidth}'.");
                        if (imageHeight <= 0 || imageHeight > pageHeight)
                            throw new ArgumentException($"Invalid image height value: '{imageHeight}'. The height of the image must be greater than 0 and less than the height of the page: '{pageHeight}'.");

                        var imageX = field.X ?? 0;
                        var y = field.Y ?? 0;
                        var imageY = pageHeight - y - imageHeight;

                        image.SetFixedPosition(pageNum, imageX, imageY);//The center of the coordinate system is the bottom-left corner

                        document.Add(image);
                    }
                    else //Text box
                    {
                        var text = new Text(stringValue);
                        var paragraph = new Paragraph(text);

                        //position
                        var textX = field.X ?? 0;
                        var y = field.Y ?? 0;

                        if (textX < 0 || textX > pageWidth)
                        throw new ArgumentException($"Invalid text coordinate X value: '{textX}'. The value must be greater than 0 and less than the width of the page: '{pageWidth}'.");
                        if (y < 0 || y > pageHeight)
                            throw new ArgumentException($"Invalid text coordinate Y value: '{y}'. The value must be greater than 0 and less than the height of the page: '{pageHeight}'.");

                        if (field.Width != null)
                        {
                            var textRectWidth = field.Width.Value;
                            if (textRectWidth <= 0 || textRectWidth > pageWidth - textX)
                                throw new ArgumentException($"Invalid text width value: '{textRectWidth}'. The width must be greater than 0 and less than: '{pageWidth - textX}'.");
                            paragraph.SetWidth(textRectWidth);
                        }
                        else
                            paragraph.SetWidth(pageWidth - textX);

                        if (field.Height != null)
                        {
                            var textRectHeight = field.Height.Value;
                            if (textRectHeight <= 0 || textRectHeight > pageHeight)
                                throw new ArgumentException($"Invalid text height value: '{textRectHeight}'. The height must be greater than 0 and less than the height of the page: '{pageHeight}'.");
                            paragraph.SetHeight(textRectHeight);
                        }
                        else
                            paragraph.SetHeight(pageHeight - y);
                        
                        var textY = pageHeight - y - paragraph.GetHeight().GetValue();
                        paragraph.SetFixedPosition(pageNum, textX, textY, paragraph.GetWidth().GetValue());

                        //
                        var textStyle = field.TextStyle;
                        if (textStyle != null) 
                        {
                            paragraph.SetMultipliedLeading(textStyle.Leading.HasValue ? textStyle.Leading.Value : 1);

                            FontStyle? fontStyle = textStyle.Font;
                            if (fontStyle != null)
                            {
                                if (fontStyle.Size.HasValue)
                                    paragraph.SetFontSize(fontStyle.Size.Value);

                                if (fontStyle.Name != null)
                                {
                                    if (!fontsHelper.FontExists(fontStyle.Name))
                                        throw new ArgumentException($"The font with the name '{fontStyle.Name}' not found.");

                                    PdfFont? font = fontsHelper.CreateFont(fontStyle.Name);
                                    if (font != null)
                                        paragraph.SetFont(font);
                                }

                                if ((fontStyle.Bold ?? false) && !fontsHelper.IsFontBold(fontStyle.Name))
                                    text.SimulateBold();

                                if ((fontStyle.Italic ?? false) && !fontsHelper.IsFontItalic(fontStyle.Name))
                                    text.SimulateItalic();

                                if (fontStyle.Underline ?? false)
                                    text.SetUnderline();
                            }

                            if (textStyle.HorizontalAlignment.HasValue)
                            {
                                if (Enum.TryParse<iTextProperties.TextAlignment>(textStyle.HorizontalAlignment.Value.ToString(), true, out iTextProperties.TextAlignment horizontalAlignment))
                                    paragraph.SetTextAlignment(horizontalAlignment);
                                else
                                    _logger.Warn($"Unknown text HorizontalAlignment value '{textStyle.HorizontalAlignment.Value}'.");
                            }

                            if (textStyle.VerticalAlignment.HasValue)
                            {
                                if (Enum.TryParse<iTextProperties.VerticalAlignment>(textStyle.VerticalAlignment.Value.ToString(), true, out iTextProperties.VerticalAlignment verticalAlignment))
                                    paragraph.SetVerticalAlignment(verticalAlignment);
                                else
                                    _logger.Warn($"Unknown text VerticalAlignment value '{textStyle.VerticalAlignment.Value}'.");
                            }

                            if (textStyle.Color != null)
                            {
                                if (!colorConverter.IsValid(textStyle.Color))
                                    throw new ArgumentException($"Unknown color value '{textStyle.Color}'.");

                                var color = (System.Drawing.Color)colorConverter.ConvertFromInvariantString(textStyle.Color)!;
                                paragraph.SetFontColor(new DeviceRgb(color.R, color.G, color.B), color.A / 255f);
                            }
                        }

                        document.Add(paragraph);
                    }
                }
            }
        }

        // try to recalculate the form using javascript        
        pdfDocument.GetCatalog().SetOpenAction(PdfAction.CreateJavaScript("this.calculateNow();"));
        pdfDocument.Close();

        if (outputStream != null)
            return outputStream.GetBuffer();
        else
            throw new InvalidOperationException("The output stream is null");
    }

    private bool IsBase64String(string input, out Span<byte> buffer)
    {
        buffer = null;

        if (string.IsNullOrEmpty(input))
            return false;

        if (input.Length % 4 != 0)
            return false;

        var base64Regex = new Regex(@"^[A-Za-z0-9+/]*={0,2}$", RegexOptions.None);
        if (!base64Regex.IsMatch(input))
            return false;

        buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer, out _);
    }

    private PdfDocument OpenPdfDocument(IFormFile pdfFile, PdfFileOpenMode mode, out ByteArrayOutputStream? outputStream)
    {
        outputStream = null;
        PdfReader reader = new PdfReader(pdfFile.OpenReadStream());

        if (mode == PdfFileOpenMode.Read)
            return new PdfDocument(reader);
        else
        {
            outputStream = new ByteArrayOutputStream();
            return new PdfDocument(reader, new PdfWriter(outputStream));
        }
    }

    private List<PdfField> ReadAllFormFields(PdfDocument pdfDocument, bool createIfNotExist)
    {
        List<PdfField> fieldsList = new List<PdfField>();

        PdfAcroForm acroForm = PdfFormCreator.GetAcroForm(pdfDocument, createIfNotExist);

        if (acroForm != null)
        {
            IDictionary<string, PdfFormField> formFields = acroForm.GetAllFormFields();

            foreach (var formField in formFields)
            {
                PdfField field = FieldFactory.Create(formField.Value);
                fieldsList.Add(field);                
            }
        }

        return fieldsList;
    }

    private List<PdfPageInfo> ReadAllPagesInfo(PdfDocument pdfDocument)
    {
        List<PdfPageInfo> pageInfoList = new List<PdfPageInfo>();

        int pagesCount = pdfDocument.GetNumberOfPages();
        for (int i = 1; i <= pagesCount; i++)
            pageInfoList.Add(new PageInfo(pdfDocument.GetPage(i)));

        return pageInfoList;
    }
}

public enum PdfFileOpenMode
{
    Read,
    ReadWrite
}
