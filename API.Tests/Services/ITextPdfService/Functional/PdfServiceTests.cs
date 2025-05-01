using API.Models.Requests;
using API.Services.ITextPdfService;
using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using Microsoft.AspNetCore.Http;
using Moq;


namespace API.Tests.Services.ITextPdfService.Functional.Tests;

// This class tests real functionality of ITextPdfService
public class PdfServiceTest
{
    private readonly PdfService _pdfService;

    public PdfServiceTest()
    {
        _pdfService = new PdfService();
    }

    [Fact(DisplayName = "ReadFields should return fields from read PDF")]
    public void ReadFields_ShouldReturnFields()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>
        {
            new FillRequest.Field { Name = "field1" },
            new FillRequest.Field { Name = "field2" },
            new FillRequest.Field { Name = "field3", Value = "value 3" }
        });

        // Act
        var fields = _pdfService.ReadFields(pdfFile.Object).fields;

        // Assert
        Assert.NotNull(fields);
        Assert.Equal(3, fields.Count);
        Assert.Contains(fields, f => f.Name == "field1");
        Assert.Contains(fields, f => f.Name == "field2");
        Assert.Contains(fields, f => f.Name == "field3" && f.Value is string && f.Value?.ToString() == "value 3");        
    }

    [Fact(DisplayName = "ReadFields should return pages info from read PDF")]
    public void ReadFields_ShouldReturnPagesInfo()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>());

        // Act
        var pages = _pdfService.ReadFields(pdfFile.Object).pages;

        // Assert
        Assert.NotNull(pages);
        Assert.Single(pages);

        var pageInfo = pages.First();
        Assert.Equal(1, pageInfo.Number);

        Assert.NotNull(pageInfo.Width);
        Assert.True(pageInfo.Width > 0);

        Assert.NotNull(pageInfo.Height);
        Assert.True(pageInfo.Height > 0);
    }

    [Fact(DisplayName = "Fill should fill fields in real PDF")]
    public void Fill_ShouldFillFields()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>
        {
            new FillRequest.Field { Name = "field1" },
            new FillRequest.Field { Name = "field2" },
            new FillRequest.Field { Name = "field3", Value = "value 3" }
        });
        var fieldsToFill = new List<FillRequest.Field>
        {
            new FillRequest.Field { Name = "field1", Value = "value1" },
            new FillRequest.Field { Name = "field2", Value = "value2" },
            new FillRequest.Field { Name = "field3", Value = "newvalue3" }
        };

        // Act
        var filledPdfBytes = _pdfService.Fill(pdfFile.Object, fieldsToFill);

        // Assert
        Assert.NotNull(filledPdfBytes);
        using var pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(filledPdfBytes)));
        var acroForm = PdfAcroForm.GetAcroForm(pdfDocument, false);
        Assert.NotNull(acroForm);
        var formFields = acroForm.GetAllFormFields();
        Assert.Equal("value1", formFields["field1"].GetValueAsString());
        Assert.Equal("value2", formFields["field2"].GetValueAsString());
        Assert.Equal("newvalue3", formFields["field3"].GetValueAsString());
    }

    [Fact(DisplayName = "Fill should throw an error when the field doesn't exist in the form")]
    public void Fill_ShouldThrowError_WhenFieldDoesNotExist()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>
        {
            new FillRequest.Field { Name = "field1" },
            new FillRequest.Field { Name = "field2" }
        });
        var fieldsToFill = new List<FillRequest.Field>
        {
            new FillRequest.Field { Name = "field1", Value = "value1" },
            new FillRequest.Field { Name = "nonexistentField", Value = "value2" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _pdfService.Fill(pdfFile.Object, fieldsToFill));
        Assert.Contains("'nonexistentField' not found", exception.Message);
    }

    [Fact(DisplayName = "Fill should throw an error when the page number is incorrect")]
    public void Fill_ShouldThrowError_WhenIncorrectPageNum()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>());
        var fieldsToFill = new List<FillRequest.Field>
        {
            new FillRequest.Field { Page = 2, Value = "Field 2 value" }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _pdfService.Fill(pdfFile.Object, fieldsToFill));
        Assert.Contains("Incorrect page number: '2'", exception.Message);
    }

    [Fact(DisplayName = "Fill should add an image in a real PDF")]
    public void Fill_ShouldAddImage()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>());

        var fieldsToFill = new List<FillRequest.Field>{
            new FillRequest.Field { Page = 1, Value = ImageBase64String }
        };

        // Act & Assert
        var filledPdfBytes = _pdfService.Fill(pdfFile.Object, fieldsToFill);
        Assert.NotNull(filledPdfBytes);

        using var pdfDocument = new PdfDocument(new PdfReader(new MemoryStream(filledPdfBytes)));

        PdfResources resources = pdfDocument.GetPage(1).GetResources();
        PdfDictionary xobjects = resources.GetResource(PdfName.XObject);
        Assert.NotNull(xobjects);

        PdfObject obj = xobjects.Get(new PdfName("Im1"));
        Assert.NotNull(obj);

        PdfImageXObject img = new PdfImageXObject((PdfStream)obj);
        Assert.Equal("png", img.IdentifyImageFileExtension());
    }

    [Fact(DisplayName = "Fill should throw an error when the image scale is incorrect")]
    public void Fill_ShouldThrowError_WhenIncorrectImageSize()
    {
        // Arrange
        var pdfFile = CreateSimplePdfForm(new List<FillRequest.Field>());

        var fistPage = _pdfService.ReadFields(pdfFile.Object).pages[0];
        var pageWidth = fistPage.Width;
        var pageHeight = fistPage.Height;

        pdfFile.Object.OpenReadStream().Position = 0;
        var fieldsToFill = new List<FillRequest.Field>{
            new FillRequest.Field { Page = 1, Value = ImageBase64String, Scale = 0 }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _pdfService.Fill(pdfFile.Object, fieldsToFill));
        Assert.Contains("Invalid image scale value: '0'", exception.Message);

        // Arrange
        pdfFile.Object.OpenReadStream().Position = 0;
        var scale = (pageWidth + 100) / 50;
        fieldsToFill = new List<FillRequest.Field>{
            new FillRequest.Field { Page = 1, Value = ImageBase64String, Scale = scale }
        };

        // Act & Assert
        exception = Assert.Throws<ArgumentException>(() => _pdfService.Fill(pdfFile.Object, fieldsToFill));
        Assert.Contains("Invalid image width value", exception.Message);

        // Arrange
        pdfFile.Object.OpenReadStream().Position = 0;
        fieldsToFill = new List<FillRequest.Field>{
            new FillRequest.Field { Page = 1, Value = ImageBase64String, Width = pageWidth + 50 , Height = pageHeight + 100 }
        };

        // Act & Assert
        exception = Assert.Throws<ArgumentException>(() => _pdfService.Fill(pdfFile.Object, fieldsToFill));
        Assert.Contains("Invalid image width value", exception.Message);
    }

    //Black rectangle 50x30 (.png)
    private readonly string ImageBase64String = "iVBORw0KGgoAAAANSUhEUgAAADIAAAAeCAIAAADhM9qrAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAcSURBVFhH7cExAQAAAMKg9U9tDQ8gAAAAAK7UABGyAAFALqTGAAAAAElFTkSuQmCC";

    private Mock<IFormFile> CreateSimplePdfForm(List<FillRequest.Field> fields)
    {
        var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        // do not close stream to keep it open for reading in tests
        writer.SetCloseStream(false);
        var pdfDocument = new PdfDocument(writer);
        var document = new iText.Layout.Document(pdfDocument);

        var acroForm = PdfAcroForm.GetAcroForm(pdfDocument, true);

        foreach (var field in fields)
        {
            var textField = new TextFormFieldBuilder(pdfDocument, field.Name)
                .SetWidgetRectangle(new iText.Kernel.Geom.Rectangle(50, 100 + fields.IndexOf(field) * 100, 200, 20))
                .CreateText()
                .SetValue(field.Value?.ToString());
            acroForm.AddField(textField);
        }

        document.Close();
        stream.Position = 0;

        var pdfFileMock = new Mock<IFormFile>();
        pdfFileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        pdfFileMock.Setup(f => f.Length).Returns(stream.Length);
        pdfFileMock.Setup(f => f.FileName).Returns("test.pdf");
        pdfFileMock.Setup(f => f.ContentType).Returns("application/pdf");

        return pdfFileMock;
    }
}
