using API.Security;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    readonly IApiVersionDescriptionProvider provider;

    // must be the same as in your middleware for API key authentication
    public const string ApiKeyHeaderName = "X-Api-Key";

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) => this.provider = provider;

    
    public void Configure(SwaggerGenOptions options)
    {
        // add a swagger document for each discovered API version
        // note: you might choose to skip or document deprecated API versions differently
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));            
        }        
    }

    public static void AddSwaggerGen(SwaggerGenOptions options) {
        
        // https://github.com/swagger-api/swagger-ui/issues/7911
        options.CustomSchemaIds(type => type.FullName?.Replace("+", ".")); // Use full type names in schema definitions);    
        options.OperationFilter<SwaggerDefaultValues>();

        // add API key authorization to swagger UI
        options.AddSecurityDefinition(ApiKeyHeaderName, new OpenApiSecurityScheme
        {
            Description = "API Key header using the ApiKey scheme",
            In = ParameterLocation.Header,
            Name = ApiKeyHeaderName,
            Type = SecuritySchemeType.ApiKey,
            Scheme = ApiKeyAuthenticationOptions.DefaultScheme
        });
    }

    static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = "PdfService API",
            Version = description.ApiVersion.ToString()            
        };

        if (description.IsDeprecated)
            info.Description += " This API version has been deprecated.";

        return info;
    }
}