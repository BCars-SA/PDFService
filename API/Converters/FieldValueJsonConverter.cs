using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace API.Converters;

/**
    * The FieldValueJsonConverter class is a custom JSON converter for the FieldValue model.
    * It is used to convert the value of the field from the JSON format, as it can be of different types: string, int, double, bool, string[], int[].
    */
public class FieldValueJsonConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            JsonArray array = JsonArray.Parse(ref reader)?.AsArray() ?? new JsonArray();

            if (array.Count == 0)
                return new List<string>();

            var firstArrayValueKind = array[0]?.GetValueKind() ?? JsonValueKind.String;

            try
            {
                if (firstArrayValueKind == JsonValueKind.String)
                    return JsonSerializer.Deserialize<List<string>>(array, options);

                if (firstArrayValueKind == JsonValueKind.Number)
                    return JsonSerializer.Deserialize<List<int>>(array, options);
            }
            catch
            {
                throw new JsonException("The array of values in 'Value' ​​can be of one of the types: string[], int[]");
            }
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int intValue))
                return intValue;
            if (reader.TryGetDouble(out double doubleValue))
                return doubleValue;

            return reader.GetString();
        }

        if (reader.TokenType == JsonTokenType.True)
            return true;

        if (reader.TokenType == JsonTokenType.False)
            return false;

        throw new JsonException($"Unexpected token type: '{reader.TokenType}'");
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {        
        switch (value)
        {            
            case List<string> stringList:
                JsonSerializer.Serialize(writer, stringList, options);
                break;
            case List<int> intList:
                JsonSerializer.Serialize(writer, intList, options);
                break;
            case int intValue:
                writer.WriteNumberValue(intValue);
                break;
            case double doubleValue:
                writer.WriteNumberValue(doubleValue);
                break;
            case bool boolValue:
                writer.WriteBooleanValue(boolValue);
                break;
            default:
                writer.WriteStringValue(value.ToString());
                break;        
        }
    }
}
