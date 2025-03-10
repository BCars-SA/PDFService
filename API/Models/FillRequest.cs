using System.Text.Json;
using System.Text.Json.Serialization;
using API.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

[ModelBinder(BinderType = typeof(FillRequestBinder))]
public class FillRequest
{
    public IFormFile? file { get; set; }
    public FieldsData? data { get; set; }

    public class FieldsData
    {
        public List<Field>? Fields { get; set; }
    }

    public class Field
    {
        public string? Name { get; set; } = null;

        [JsonConverter(typeof(FieldValueJsonConverter))]
        public object? Value { get; set; }

        public string? DisplayValue { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public string? Type { get; set; }
        public int? Page { get; set; }
    }
}

/**
    * The FillRequestBinder class is a custom model binder for the FillRequest model.
    * It is used to bind the FillRequest model from the request data.
    */
public class FillRequestBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext == null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        var fillRequest = new FillRequest() {
            data = new FillRequest.FieldsData()
        };
        foreach (var key in bindingContext.HttpContext.Request.Form.Keys)
        {
            if (key == "data")
            {
                try
                {
                    var value = bindingContext.HttpContext.Request.Form[key].ToString();
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
                    throw new InvalidOperationException($"The request data json deserialization error: [{ex.Message}]");
                }
            }
        }
        fillRequest.file = bindingContext.HttpContext.Request.Form.Files["file"];
        bindingContext.Result = ModelBindingResult.Success(fillRequest);
        return Task.CompletedTask;
    }
}