using API.Controllers;
using API.Models.Pdf;
using API.Models.Requests;
using API.Models.Responses;
using API.Services;
using API.Tests.Models.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

namespace API.Tests.Controllers
{
    public class PdfControllerTests
    {
        private readonly Mock<IPdfService> _pdfServiceMock;
        private readonly PdfController _controller;

        public PdfControllerTests()
        {
            var configurationMock = new Mock<IConfiguration>();
            _pdfServiceMock = new Mock<IPdfService>();
            _controller = new PdfController(configurationMock.Object, _pdfServiceMock.Object);
        }

        [Fact(DisplayName = "FillDocument returns BadRequest when file is null")]
        public void FillDocument_ReturnsBadRequest_WhenFileIsNull()
        {
            // Arrange
            var request = new FillRequest { file = null!, data = new FillRequest.FieldsData() };
            
            // Act
            var result = _controller.FillDocument(request);

            // Assert
            var problemDetails = Assert.IsType<ProblemDetails>((result as ObjectResult)?.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("'file' was expected in the form", problemDetails.Detail);                          
        }

        [Fact(DisplayName = "FillDocument returns BadRequest when data fields are null or empty")]
        public void FillDocument_ReturnsBadRequest_WhenDataFieldsAreNullOrEmpty()
        {
            // Arrange
            var request = new FillRequest { file = new Mock<IFormFile>().Object, data = new FillRequest.FieldsData() { Fields = null } };

            // Act
            var result = _controller.FillDocument(request);

            // Assert            
            var problemDetails = Assert.IsType<ProblemDetails>((result as ObjectResult)?.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("'data.fields' was expected in the form", problemDetails.Detail);            
        }

        [Fact(DisplayName = "FillDocument returns FileContentResult when successful")]
        public void FillDocument_ReturnsFileContentResult_WhenSuccessful()
        {
            // Arrange
            var request = new FillRequest
            {
                file = new Mock<IFormFile>().Object,
                data = new FillRequest.FieldsData { Fields = new List<FillRequest.Field> { new FillRequest.Field() } }
            };
            var memoryStream = new MemoryStream();
            _pdfServiceMock.Setup(service => service.Fill(It.IsAny<IFormFile>(), It.IsAny<List<FillRequest.Field>>()))
                           .Returns(memoryStream.ToArray());

            // Act
            var result = _controller.FillDocument(request);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
        }

        [Fact(DisplayName = "FillDocument returns FieldsResponse when successful")]
        public void ReadFields_ReturnsFieldsResponse_WhenSuccessful()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();            
            var fields = new List<PdfField> { new TestPdfField("Field1", "Text", 1, false, "FieldValue1") };
            _pdfServiceMock.Setup(service => service.ReadFields(It.IsAny<IFormFile>())).Returns(fields);

            // Act
            var result = _controller.ReadFields(fileMock.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<FieldsResponse>(okResult.Value);
            Assert.NotNull(response.Fields);
            Assert.Single(response.Fields);
            Assert.Equal("Field1", response.Fields[0].Name);
        }

        [Fact(DisplayName = "ReadFields returns BadRequest when any exception thrown")]
        public void ReadFields_ReturnsBadRequest_WhenExceptionThrown()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            _pdfServiceMock.Setup(service => service.ReadFields(It.IsAny<IFormFile>())).Throws(new System.Exception("Error"));

            // Act
            var result = _controller.ReadFields(fileMock.Object);

            // Assert            
            var problemDetails = Assert.IsType<ProblemDetails>((result as ObjectResult)?.Value);
            Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
            Assert.Equal("Error", problemDetails.Detail);
        }
    }
}