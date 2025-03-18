using System.Text.Json;
using System.Text.Json.Serialization;
using API.Converters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    readonly string fileKey = "file";
    readonly string dataKey = "data";

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
                try
                {
                    var value = form[key].ToString();
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
                    bindingContext.ModelState.AddModelError(dataKey, $"The request '{dataKey}' json deserialization error: " + ex.Message);
                    bindingContext.Result = ModelBindingResult.Failed();            
                    return Task.CompletedTask;                    
                }
            }
        }        

        bindingContext.Result = ModelBindingResult.Success(fillRequest);
        return Task.CompletedTask;
    }
}