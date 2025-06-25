using API.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Models.Requests;

[ModelBinder(BinderType = typeof(FillRequestBinder))]
public class FillRequest
{
    public required IFormFile file { get; set; }

    public required FieldsData data { get; set; }

    public class FieldsData
    {
        public List<Field>? Fields { get; set; }
    }

    public class Field
    {
        public string? Name { get; set; } = null;

        [JsonConverter(typeof(FieldValueJsonConverter))]
        public object? Value { get; set; }

        public float? X { get; set; }
        public float? Y { get; set; }
        public float? Scale { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public string? Type { get; set; }
        public TextStyle? TextStyle { get; set; }
        public int? Page { get; set; }
    }

    public class TextStyle
    {
        public float? Leading { get; set; }
        public string? Color { get; set; }

        [JsonConverter(typeof(EnumJsonConverter<TextHorizontalAlignment>))]
        public TextHorizontalAlignment? HorizontalAlignment { get; set; }

        [JsonConverter(typeof(EnumJsonConverter<TextVerticalAlignment>))]
        public TextVerticalAlignment? VerticalAlignment { get; set; }

        public FontStyle? Font { get; set; }
    }

    public enum TextHorizontalAlignment
    {
        LEFT,
        CENTER,
        RIGHT
    }

    public enum TextVerticalAlignment
    {
        TOP,
        MIDDLE,
        BOTTOM
    }

    public class FontStyle
    {
        public string? Name { get; set; } = null;
        public float? Size { get; set; }
        public bool? Bold { get; set; }
        public bool? Italic { get; set; }
        public bool? Underline { get; set; }
    }
}

/**
    * The FillRequestBinder class is a custom model binder for the FillRequest model.
    * It is used to bind the FillRequest model from the request data.
    */
public class FillRequestBinder : IModelBinder
{
    readonly string fileKey = "file";
    readonly string dataKey = "data";

    readonly long maxInvalidJsonPartToLogLength = 50;

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        IFormCollection? form = null;
        try {
            form = bindingContext.HttpContext.Request.Form;
        } catch (Exception) {
            bindingContext.ModelState.AddModelError(fileKey, $"'{fileKey}' form data was expected");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        if (form == null || form.Files[fileKey] == null) 
        {
            bindingContext.ModelState.AddModelError(fileKey, $"'{fileKey}' form data was expected");
            bindingContext.Result = ModelBindingResult.Failed();
            return Task.CompletedTask;
        }

        var fillRequest = new FillRequest() {
            file = form.Files[fileKey]!,
            data = new FillRequest.FieldsData()
        };

        foreach (var key in form.Keys)
        {
            if (key == dataKey)
            {
                string value = "";

                try
                {
                    value = form[key].ToString();

                    fillRequest.data.Fields = JsonSerializer.Deserialize<FillRequest.FieldsData>(
                        value,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    )?.Fields ?? new List<FillRequest.Field>();
                }
                catch (Exception ex)
                {
                    var errorMessage = $"The request '{dataKey}' json deserialization error: '{ex.Message}'.";
                    string? jsonPartToAddToError = null;

                    if (ex is JsonException && !string.IsNullOrEmpty(value))
                    {
                        var jsonException = (JsonException)ex;

                        if (jsonException.LineNumber.HasValue)
                        {
                            var lines = value.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                            var lineNumber = jsonException.LineNumber.Value;
                            if (lineNumber >= 0 && lineNumber < lines.Length)
                            {
                                var line = lines[lineNumber];
                                if (line.Length > maxInvalidJsonPartToLogLength)
                                {
                                    var errorPos = jsonException.BytePositionInLine ?? 0;

                                    var startIndex = Math.Max(0, errorPos - maxInvalidJsonPartToLogLength / 2);
                                    var length = Math.Min(line.Length, errorPos + maxInvalidJsonPartToLogLength / 2) - startIndex;

                                    jsonPartToAddToError = line.Substring((int)startIndex, (int)length);
                                }
                                else
                                {
                                    jsonPartToAddToError = line;
                                }
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(jsonPartToAddToError ?? value))
                        errorMessage += $" The invalid part of the json: '{jsonPartToAddToError ?? value}'.";

                    bindingContext.ModelState.AddModelError(dataKey, errorMessage);
                    bindingContext.Result = ModelBindingResult.Failed();   
                    
                    return Task.CompletedTask;                    
                }

                break;
            }
        }        

        bindingContext.Result = ModelBindingResult.Success(fillRequest);
        return Task.CompletedTask;
    }
}