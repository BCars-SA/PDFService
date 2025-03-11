using API.Models.Responses;
using API.Models.Requests;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("pdf")]
[ApiController]
public class PdfController : BaseController
{
    private readonly IPdfService _pdfService;

    public PdfController(IConfiguration configuration, IPdfService pdfService) : base(configuration)
    {
        _pdfService = pdfService;
    }

    [Route("fill")]
    [HttpPost]
    [ApiVersion("1.0")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult FillDocument([FromForm] FillRequest request)
    {
        if (request.file == null)
        {
            return BadRequestProblem("'file' was expected in the form");
        }
        if (request.data?.Fields == null || request.data?.Fields?.Count == 0)
        {
            return BadRequestProblem("'data.fields' was expected in the form");
        }

        try
        {            
            var outputStream = _pdfService.Fill(request.file, request.data!.Fields);
            return File(outputStream, "application/pdf");
        }
        catch (Exception exc)
        {
            return BadRequestProblem($"The document processing error: '{exc.Message}'");
        }        
    }

    [Route("fields")]
    [HttpPost]
    [ApiVersion("1.0")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FieldsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult ReadFields(IFormFile file)
    {
        try
        {
            var list = _pdfService.ReadFields(file);
            return Ok(new FieldsResponse()
            {
                FieldsCount = list.Count,
                Fields = list.Select(f => new FieldsResponse.Field()
                {
                    Name = f.Name,
                    Type = f.Type,
                    Page = f.Page,
                    Value = f.Value
                }).ToList()
            });
        }
        catch(Exception exc)
        {
            return BadRequestProblem(exc.Message);
        }
    }
}