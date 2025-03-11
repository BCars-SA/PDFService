using API;
using API.Services;
using API.Services.ITextPdfService;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddNLog();

// Add services to the container.
builder.Services.AddControllers(options => {
        // produce 'application/json' by default
        options.Filters.Add(new ProducesAttribute("application/json"));
    }).AddJsonOptions(options => {
        // camelCase JSON by default
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Use the versioning middleware to add API versioning to the application via the header
// The API version header is "api-version"
builder.Services.AddApiVersioning(options =>
    {
        options.ReportApiVersions = true; // Add headers to the response that indicate the supported versions
        options.DefaultApiVersion = new ApiVersion(1, 0); // Default API version is 1.0
        options.AssumeDefaultVersionWhenUnspecified = true;    
        options.ApiVersionReader = new HeaderApiVersionReader(new[] { "api-version" }); // Use HeaderApiVersionReader
    })
    .AddVersionedApiExplorer(options =>
    {
        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
        // note: the specified format code will format the version as "major[.minor]"
        options.GroupNameFormat = "VV";
        options.SubstituteApiVersionInUrl = true;    
    });

// Setup Swagger
builder.Services    
    .AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()
    .AddSwaggerGen((options) => {
        // https://github.com/swagger-api/swagger-ui/issues/7911
        options.CustomSchemaIds(type => type.FullName?.Replace("+", ".")); // Use full type names in schema definitions);    
        options.OperationFilter<SwaggerDefaultValues>();
    });


// Register PdfService
builder.Services.AddScoped<IPdfService,PdfService>();

// Parse AllowedOrigins from configuration and add CORS policy for them
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(corsBuilder =>
    {
        var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(",", StringSplitOptions.RemoveEmptyEntries);
        if (allowedOrigins?.Any() == true)
        {
            corsBuilder.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// APP configuration
var app = builder.Build();
app.UseSwagger().UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        // add swagger endpoints jsons for each discovered API version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
        }
    });

app.UseCors();
app.MapControllers();

// exceptions handler with automatic NLog logging
app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;
        var problemDetails = new ProblemDetails()
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };

        if (app.Environment.IsDevelopment())
        {
            problemDetails.Detail = exception?.Message;
            problemDetails.Extensions["stackTrace"] = exception?.StackTrace;
        }

        NLog.LogManager.GetCurrentClassLogger().Error(exception);

        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});

app.Run();
