using System.Collections.Immutable;
using API.Models.Pdf;
using API.Models.Requests;
using API.Services.ITextPdfService.Fields;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Source;
using iText.Kernel.Pdf;

namespace API.Services.ITextPdfService;

public class PdfService : IPdfService
{
    public List<PdfField> ReadFields(IFormFile pdfFile)
    {
        var pdfDocument = OpenPdfDocument(pdfFile, PdfFileOpenMode.Read, out _);
        return ReadAllFormFields(pdfDocument, false);
    }

    public byte[] Fill(IFormFile pdfFile, List<FillRequest.Field> fields)
    {
        var pdfDocument = OpenPdfDocument(pdfFile, PdfFileOpenMode.ReadWrite, out var outputStream);
        var formFields = ReadAllFormFields(pdfDocument, true);
        // field.Name is not null here because of the ReadAllFormFields method
        var fieldsDictionary = formFields.ToDictionary(f => f.Name!.ToLower(), f => f);

        foreach (var field in fields)
        {
            var fieldName = field.Name?.ToLower();
            if (fieldName != null)
            {
                if (!fieldsDictionary.ContainsKey(fieldName))
                    throw new Exception($"The document field with the name '{field.Name}' not found.");

                var pdfField = fieldsDictionary[fieldName];
                pdfField.Value = field.Value;
            }
            else
            {
                //to do
            }
        }

        pdfDocument.Close();

        if (outputStream != null)
            return outputStream.GetBuffer();
        else
            throw new InvalidOperationException("The output stream is null");
    }

    private PdfDocument OpenPdfDocument(IFormFile pdfFile, PdfFileOpenMode mode, out ByteArrayOutputStream? outputStream)
    {
        outputStream = null;
        PdfReader reader = new PdfReader(pdfFile.OpenReadStream());

        if (mode == PdfFileOpenMode.Read)
            return new PdfDocument(reader);
        else
        {
            outputStream = new ByteArrayOutputStream();
            return new PdfDocument(reader, new PdfWriter(outputStream));
        }
    }

    private List<PdfField> ReadAllFormFields(PdfDocument pdfDocument, bool createIfNotExist)
    {
        List<PdfField> fieldsList = new List<PdfField>();

        PdfAcroForm acroForm = PdfFormCreator.GetAcroForm(pdfDocument, createIfNotExist);

        if (acroForm != null)
        {
            IDictionary<string, PdfFormField> formFields = acroForm.GetAllFormFields();

            foreach (var formField in formFields)
            {
                PdfField field = FieldFactory.Create(formField.Value);
                fieldsList.Add(field);                
            }
        }

        return fieldsList;
    }
}

public enum PdfFileOpenMode
{
    Read,
    ReadWrite
}
