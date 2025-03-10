using API.Models.Pdf;
using API.Models.Pdf.Fields;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("pdf")]
[ApiController]
public class PdfController : BaseController
{
    public PdfController(IConfiguration configuration) : base(configuration)
    {
    }

    [Route("fill")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public IActionResult FillDocument([FromForm] FillRequest request)
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

        try {
            var outputStream = pdfFile.GetOuputStream();
            if (outputStream == null)
                throw new InvalidOperationException("The output stream is null");
            return File(outputStream, "application/pdf");
        }
        catch (Exception exc)
        {
            return BadRequestProblem($"The document processing error: '{exc.Message}'");
        }        
    }

    [Route("fields")]
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(FieldsResponse<FormField>), StatusCodes.Status200OK)]
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

        return Ok(new FieldsResponse<FormField>()
        {
            FieldsCount = pdfFile.Fields.Count,
            Fields = pdfFile.Fields
        });
    }
}