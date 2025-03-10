using API.Models.Pdf.Fields;
using API.Models.Requests;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Source;
using iText.Kernel.Pdf;

namespace API.Services;

public interface IPdfService {    
    List<FormField> ReadFields(IFormFile pdfFile);
    byte[] Fill(IFormFile pdfFile, List<FillRequest.Field> fields);
}

public class PdfService : IPdfService
{
    public List<FormField> ReadFields(IFormFile pdfFile)
    {
        var pdfDocument = OpenPdfDocument(pdfFile, PdfFileOpenMode.Read, out _);
        return ReadAllFormFields(pdfDocument, false);
    }
    
    public byte[] Fill(IFormFile pdfFile, List<FillRequest.Field> fields)
    {
        var pdfDocument = OpenPdfDocument(pdfFile, PdfFileOpenMode.ReadWrite, out var outputStream);
        var formFields = ReadAllFormFields(pdfDocument, true);
        var fieldsDictionary = formFields.ToDictionary(f => f.Name.ToLower(), f => f);        
        
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

    private List<FormField> ReadAllFormFields(PdfDocument pdfDocument, bool createIfNotExist)
    {        
        List<FormField> fieldsList = new List<FormField>();

        PdfAcroForm acroForm = PdfFormCreator.GetAcroForm(pdfDocument, createIfNotExist);

        if (acroForm != null)
        {
            IDictionary<String, PdfFormField> formFields = acroForm.GetAllFormFields();

            foreach (var formField in formFields)
            {
                FormField field = FieldFactory.Create(formField.Value);
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
