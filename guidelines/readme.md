# Guidelines

## Useful links
- [The API checklist](https://mathieu.fenniak.net/the-api-checklist/)
- [Microsoft Azure REST API Guidelines](https://github.com/microsoft/api-guidelines/blob/vNext/azure/Guidelines.md)


## Auto-documentation with OpenAPI
Use OpenAPI (Swagger) to generate the documentation:\
[ASP.NET Core web API documentation with Swagger / OpenAPI](https://learn.microsoft.com/en-us/aspnet/core/tutorials/web-api-help-pages-using-swagger?view=aspnetcore-8.0)

## URI naming
DO use kebab-casing for URL path segments. If the segment refers to a JSON field, use camel casing.

## API Versioning
With `api-version` header using Microsoft.AspNetCore.Mvc.Versioning package.

``` C#
# Program.cs
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = new HeaderApiVersionReader(new[] { "api-version" }); // Use HeaderApiVersionReader
});

# Some controller
[Route("products")]
public class ProductsController : ControllerBase
{
  // GET v1/products
  [HttpGet]
  [ApiVersion("1.0")]
  public IActionResult GetV1()
  {
    // Implementation for version 1
  }

  // GET v2/products
  [HttpGet]
  [ApiVersion("2.0")]
  public IActionResult GetV2()
  {
    // Implementation for version 2
  }
}
```


## HTTP Request / Response Pattern
The HTTP Request / Response pattern dictates how your API behaves. 
For example: POST methods that create resources must be idempotent, GET method results may be cached, the If-Modified and ETag headers offer optimistic concurrency. 
The URL of a service, along with its request/response bodies, establishes the overall contract that developers have with your service. 
As a service provider, how you manage the overall request / response pattern should be one of the first implementation decisions you make.

Implement services in an idempotent manner, with an "exactly once" semantic, enables developers to retry requests without the risk of unintended consequences.

✅ DO ensure that all HTTP methods are idempotent.

☑️ YOU SHOULD use PUT or PATCH to create a resource as these HTTP methods are easy to implement, allow the customer to name their own resource, and are idempotent.


### HTTP Return Codes
✅ DO adhere to the return codes in the following table when the method completes synchronously and is successful:

Method | Description | Response Status Code
-------|-------------|---------------------
PATCH  | Create/Modify the resource with JSON Merge Patch | `200-OK`, `201-Created`
PUT    | Create/Replace the _whole_ resource | `200-OK`, `201-Created`
POST   | Create new resource (ID set by service) | `201-Created` with URL of created resource
POST   | Action | `200-OK`
GET    | Read (i.e. list) a resource collection | `200-OK`
GET    | Read the resource | `200-OK`
DELETE | Remove the resource | `204-No Content`\; avoid `404-Not Found`

✅ DO return status code `202-Accepted` and follow the guidance in Long-Running Operations & Jobs when a PUT, POST, or DELETE method completes asynchronously.

✅ DO treat method names as case sensitive and should always be in uppercase

✅ DO return the state of the resource after a PUT, PATCH, POST, or GET operation with a `200-OK` or `201-Created`.

✅ DO return a `204-No` Content without a resource/body for a DELETE operation (even if the URL identifies a resource that does not exist; do not return `404-Not Found`)

✅ DO return a `200-OK` from a POST Action. Include a body in the response, even if it has not properties, to allow properties to be added in the future if needed.

✅ DO return a `403-Forbidden` when the user does not have access to the resource unless this would leak information about the existence of the resource that should not be revealed for security/privacy reasons, in which case the response should be `404-Not Found`.

✅ DO support caching and optimistic concurrency by honoring the the `If-Match`, `If-None-Match`, if-modified-since, and if-unmodified-since request headers and by returning the ETag and last-modified response headers
