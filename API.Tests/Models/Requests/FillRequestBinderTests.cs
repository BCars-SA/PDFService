using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using Xunit;

namespace API.Models.Requests.Tests
{
    public class FillRequestTest
    {
        private DefaultModelBindingContext GetModelBindingContext(FormCollection formCollection) {
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(x => x.Request.Form).Returns(formCollection);
            return new DefaultModelBindingContext
            {
                ActionContext = new ActionContext
                {
                    HttpContext = httpContextMock.Object
                },
                ModelState = new ModelStateDictionary(),
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(typeof(FillRequest)),
                ModelName = "fillRequest"
            };
        }

        private IFormFile GetFileMockObject(string key = "file", string fileName = "file.pdf") {
            var formFileMock = new Mock<IFormFile>();
            formFileMock.Setup(f => f.Name).Returns(key);
            formFileMock.Setup(f => f.FileName).Returns(fileName);
            return formFileMock.Object; 
        }

        private FillRequest.FieldsData GetDataMockObject() {            
            return  new FillRequest.FieldsData
            {
                Fields = new List<FillRequest.Field>{ new FillRequest.Field{ Name = "TestField" }}
            };
        }

        [Fact(DisplayName = "FillRequest BindModel should bind model when valid data provided")]
        public async Task BindModelAsync_ShouldBindModel_WhenValidDataProvided()
        {
            // Arrange            
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "data", JsonSerializer.Serialize(GetDataMockObject())}
            }, new FormFileCollection { GetFileMockObject() });
            
            var modelBindingContext = GetModelBindingContext(formCollection);
            var binder = new FillRequestBinder();

            // Act
            await binder.BindModelAsync(modelBindingContext);

            // Assert
            var result = Assert.IsType<FillRequest>(modelBindingContext.Result.Model);
            Assert.NotNull(result.file);
            Assert.NotNull(result.data);            
            Assert.Equal("TestField", result.data.Fields?[0].Name);
        }

        [Fact(DisplayName = "FillRequest BindModel should throw exception when file not provided")]
        public async Task BindModelAsync_ShouldThrowException_WhenFileNotProvided()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "data", JsonSerializer.Serialize(GetDataMockObject())}
            });

            var modelBindingContext = GetModelBindingContext(formCollection);
            var binder = new FillRequestBinder();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => binder.BindModelAsync(modelBindingContext));
            Assert.Contains("'file' form data was expected", exception.Message);
        }

        [Fact(DisplayName = "FillRequest BindModel should throw exception when file provided with a wrong key")]
        public async Task BindModelAsync_ShouldThrowException_WhenFileProvidedWrongKey()
        {
            // Arrange
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "data", JsonSerializer.Serialize(GetDataMockObject())}
            }, new FormFileCollection { GetFileMockObject("newfile") });

            var modelBindingContext = GetModelBindingContext(formCollection);
            var binder = new FillRequestBinder();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => binder.BindModelAsync(modelBindingContext));
            Assert.Contains("'file' form data was expected", exception.Message);
        }

        [Fact(DisplayName = "FillRequest BindModel should throw exception when data deserialization fails")]
        public async Task BindModelAsync_ShouldThrowException_WhenDataDeserializationFails()
        {
            // Arrange            
            var formCollection = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "data", "invalid json" }
            }, new FormFileCollection { GetFileMockObject() });

            var modelBindingContext = GetModelBindingContext(formCollection);
            var binder = new FillRequestBinder();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => binder.BindModelAsync(modelBindingContext));
            Assert.Contains("json deserialization error", exception.Message);
        }
    }
}