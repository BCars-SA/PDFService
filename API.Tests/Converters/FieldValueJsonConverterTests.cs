using API.Converters;
using System.Text.Json;

namespace API.Tests.Converters
{
    public class FieldValueJsonConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public FieldValueJsonConverterTests()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new FieldValueJsonConverter() }
            };
        }

        [Theory(DisplayName = "Read single value returns expected value")]
        [InlineData("\"stringValue\"", "stringValue")]
        [InlineData("123", 123)]
        [InlineData("123.45", 123.45)]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void Read_SingleValue_ReturnsExpectedValue(string json, object expected)
        {
            var result = JsonSerializer.Deserialize<object>(json, _options);
            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Read string array returns expected list")]
        public void Read_StringArray_ReturnsExpectedList()
        {
            var json = "[\"value1\", \"value2\"]";
            var expected = new List<string> { "value1", "value2" };

            var result = JsonSerializer.Deserialize<object>(json, _options);

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Read int array returns expected list")]
        public void Read_IntArray_ReturnsExpectedList()
        {
            var json = "[1, 2, 3]";
            var expected = new List<int> { 1, 2, 3 };

            var result = JsonSerializer.Deserialize<object>(json, _options);

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Read empty array returns empty list<string>")]
        public void Read_EmptyArray_ReturnsEmptyList()
        {
            var json = "[]";
            var expected = new List<string>();

            var result = JsonSerializer.Deserialize<object>(json, _options);

            Assert.Equal(expected, result);
        }

        [Fact(DisplayName = "Read invalid array throws JsonException")]
        public void Read_InvalidArray_ThrowsJsonException()
        {
            var json = "[\"value1\", 2]";

            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<object>(json, _options));
        }

        [Theory(DisplayName = "Write single value serializes correctly")]
        [InlineData("stringValue", "\"stringValue\"")]
        [InlineData(123, "123")]        
        [InlineData(true, "true")]
        [InlineData(false, "false")]        
        public void Write_Value_SerializesCorrectly(object value, object result)
        {
            var json = JsonSerializer.Serialize(value, _options);
            Assert.Equal(result, json);
        }
        
        [Fact(DisplayName = "Write string array serializes correctly")]
        public void Write_StringArray_SerializesCorrectly()
        {
            var value = new List<string> { "value1", "value2" };
            var json = JsonSerializer.Serialize(value, _options);
            Assert.Equal("[\"value1\",\"value2\"]", json);
        }

        [Fact(DisplayName = "Write int array serializes correctly")]
        public void Write_IntArray_SerializesCorrectly()
        {
            var value = new List<int> { 1, 2, 3 };
            var json = JsonSerializer.Serialize(value, _options);
            Assert.Equal("[1,2,3]", json);
        }

        [Fact(DisplayName = "Write empty array serializes correctly")]
        public void Write_EmptyArray_SerializesCorrectly()
        {
            var value = new List<string>();
            var json = JsonSerializer.Serialize(value, _options);
            Assert.Equal("[]", json);
        }
    }
}