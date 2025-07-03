using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Converters;

public class EnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : Enum
{
    public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (!Enum.TryParse(typeof(TEnum), stringValue, true, out object? parsedEnum))
                throw new JsonException($"Unexpected enum value: '{stringValue}'. Expected one of the values:{string.Join(", ", Enum.GetNames(typeof(TEnum)))}.");

            return (TEnum)parsedEnum;
        }

        throw new JsonException($"Unexpected token type: '{reader.TokenType}'. Expected string type.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString().ToLower());
    }
}
