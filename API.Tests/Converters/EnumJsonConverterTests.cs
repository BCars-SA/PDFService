using API.Converters;
using System.Text.Json;
using static API.Models.Requests.FillRequest;

namespace API.Tests.Converters
{
    public class EnumJsonConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public EnumJsonConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new EnumJsonConverter<TextHorizontalAlignment>() }
            };
        }

        [Theory(DisplayName = "Read single value returns expected value")]
        [InlineData("\"left\"", TextHorizontalAlignment.LEFT)]
        [InlineData("\"CENTER\"", TextHorizontalAlignment.CENTER)]
        public void Read_SingleValue_ReturnsExpectedValue(string json, TextHorizontalAlignment expected)
        {
            var result = JsonSerializer.Deserialize<TextHorizontalAlignment>(json, _options);
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Read invalid value throws JsonException")]
        public void Read_InvalidArray_ThrowsJsonException()
        {
            var json = "\"somevalue\"";

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TextHorizontalAlignment>(json, _options));
        }

        [Theory(DisplayName = "Write Enum value serializes correctly")]
        [InlineData(TextHorizontalAlignment.LEFT, "\"left\"")]
        [InlineData(TextHorizontalAlignment.RIGHT, "\"right\"")]
        public void Write_Value_SerializesCorrectly(TextHorizontalAlignment value, string result)
        {
            var json = JsonSerializer.Serialize(value, _options);
            Assert.Equal(result, json);
        }
    }
}
