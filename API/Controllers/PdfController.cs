using API.Data;
using API.Models;
using API.Models.Pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace API.Controllers;

[Route("pdf")]
[ApiController]
public class PdfController : BaseController
{
    public PdfController(IConfiguration configuration, BaseDBContext<User>? userContext = null) : base(configuration, userContext)
    {
    }

    [Route("fill")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult FillDocument1([FromForm] FillRequest request)
    {
        if (request.file == null)
        {
            return BadRequestProblem("A PDF file was expected");
        }

        if (request.data?.Fields?.Count == 0)
        {
            return BadRequestProblem("A fields data was expected");
        }

        PdfFile pdfFile;

        try
        {
            pdfFile = new PdfFile(request.file, PdfFileOpenMode.ReadWrite);
            pdfFile.Fill(request);
        }
        catch (Exception exc)
        {
            return BadRequestProblem($"The document processing error: '{exc.Message}'");
        }

        var outputStream = pdfFile.GetOuputStream();
        if (outputStream == null)
        {
            return BadRequestProblem("The document output stream is null. Please report the problem to the developers.");
        }

        return File(outputStream, "application/pdf");
    }

    [Route("fields")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FieldsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ReadFields(IFormFile file)
    {
        PdfFile pdfFile;

        try
        {
            pdfFile = new PdfFile(file);
        }
        catch(Exception exc)
        {
            return BadRequestProblem(exc.Message);
        }

        return Ok(new FieldsResponse()
        {
            FieldsCount = pdfFile.Fields.Count,
            Fields = pdfFile.Fields.ConvertAll<Field>(field => new Field()
            {
                Name = field.Name,
                Value = field.Value,
                DisplayValue = field.DisplayValue,
                Type = field.Type,
                Page = field.Page,
                IsReadOnly = field.IsReadOnly
            })
        });
    }
}

public class FormBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        FillRequest fillRequest = new FillRequest();
        FieldsData fieldsData = new FieldsData() { Fields = new List<Field>() };
        foreach (var key in bindingContext.HttpContext.Request.Form.Keys)
        {
            if (key == "data")
            {
                try
                {
                    var value = bindingContext.HttpContext.Request.Form[key].ToString();
                    fieldsData.Fields = JsonSerializer.Deserialize<FieldsData>(
                        value,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    )?.Fields ?? new List<Field>();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"The request data json deserialization error: [{ex.Message}]");
                }
            }
        }

        IFormFile? file = bindingContext.HttpContext.Request.Form.Files["file"];

        fillRequest.data = fieldsData;
        fillRequest.file = file;

        bindingContext.Result = ModelBindingResult.Success(fillRequest);
        return Task.CompletedTask;
    }
}

[ModelBinder(BinderType = typeof(FormBinder))]
public class FillRequest
{
    public IFormFile? file { get; set; }
    public FieldsData? data { get; set; }
}

public class FieldsData
{
    public List<Field>? Fields { get; set; }
}

public class Field
{
    public string? Name { get; set; } = null;

    [JsonConverter(typeof(ValueJsonConverter))]
    public object? Value { get; set; }

    public string? DisplayValue { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public string? Type { get; set; }
    public int? Page { get; set; }
    public bool? IsReadOnly { get; set; }
}

public class ValueJsonConverter : JsonConverter<object>
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
        JsonSerializer.Serialize(writer, value, options);
    }
}

public class FieldsResponse
{
    public int FieldsCount { get; set; }
    public List<Field>? Fields { get; set; }
}
