using API.Controllers;
using API.Models.Pdf.Fields;
using iText.Forms;
using iText.Forms.Fields;
using iText.IO.Source;
using iText.Kernel.Pdf;

namespace API.Models.Pdf;

public class PdfFile
{
    protected PdfDocument _pdfDocument;
    protected ByteArrayOutputStream? _outputStream;

    protected List<FormField>? _fieldsList;
    protected Dictionary<string, FormField> _fieldsDictionary = new Dictionary<string, FormField>();
    protected PdfFileOpenMode _mode;

    protected bool IsReadOnly => _mode == PdfFileOpenMode.Read;

    public PdfFile(IFormFile pdfFile, PdfFileOpenMode mode = PdfFileOpenMode.Read)
    {
        _mode = mode;

        PdfReader reader = new PdfReader(pdfFile.OpenReadStream());

        if (mode == PdfFileOpenMode.Read)
            _pdfDocument = new PdfDocument(reader);
        else
        {
            _outputStream = new ByteArrayOutputStream();
            _pdfDocument = new PdfDocument(reader, new PdfWriter(_outputStream));
        }

        ReadAllFormFields();
    }

    private void ReadAllFormFields()
    {
        if (_fieldsList == null)
        {
            _fieldsList = new List<FormField>();

            PdfAcroForm acroForm = PdfFormCreator.GetAcroForm(_pdfDocument, !IsReadOnly);

            if (acroForm != null)
            {
                IDictionary<String, PdfFormField> formFields = acroForm.GetAllFormFields();

                foreach (var formField in formFields)
                {
                    FormField field = FieldFactory.Create(formField.Value);
                    _fieldsList.Add(field);
                    _fieldsDictionary[formField.Key.ToLower()] = field;
                }
            }
        }
    }

    public List<FormField> Fields
    {
        get
        {
            if (_fieldsList == null)
                ReadAllFormFields();

            return _fieldsList ?? new List<FormField>();
        }
    }

    public void Fill(FillRequest request)
    {
        var fields = request.data?.Fields; 
        
        if (fields != null)
        {
            foreach (var field in fields)
            {
                var fieldName = field.Name?.ToLower();
                if (fieldName != null)
                {
                    if (!_fieldsDictionary.ContainsKey(fieldName))
                        throw new Exception($"The document field whith the name '{field.Name}' not found.");

                    var pdfField = _fieldsDictionary[fieldName];
                    pdfField.Value = field.Value;
                }
                else
                {
                    //to do
                }
            }
        }

    }

    public byte[]? GetOuputStream()
    {
        _pdfDocument.Close();
        return _outputStream?.GetBuffer();
    }
}

public enum PdfFileOpenMode
{
    Read,
    ReadWrite
}
