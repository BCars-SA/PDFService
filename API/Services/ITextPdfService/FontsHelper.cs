using iText.Commons.Utils;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using NLog;
using IO = System.IO;

namespace API.Services.ITextPdfService;

class FontsHelper
{
    PdfDocument _pdfDocument;
    FontsCache _fontsCache;

    public FontsHelper(PdfDocument pdfDocument, IConfiguration? configuration, Logger logger)
    {
        _pdfDocument = pdfDocument;
        _fontsCache = FontsCache.GetInstance(configuration, logger);
    }

    public bool FontExists(string fontName)
    {
        return AllAvailableFontsKeys.Contains(fontName.Trim().ToLowerInvariant());
    }

    public bool IsFontBold(string? fontName)
    {
        return fontName != null && fontName.ToLowerInvariant().Contains("bold");
    }

    public bool IsFontItalic(string? fontName)
    {
        fontName = fontName?.ToLowerInvariant();
        return fontName != null && (fontName.Contains("italic") || fontName.Contains("oblique"));
    }

    public PdfFont? CreateFont(string fontName)
    {
        fontName = fontName.Trim().ToLowerInvariant();

        if (FontExists(fontName))
        {
            if (_fontsCache.FontExists(fontName))
            {
                return _fontsCache.CreateFont(fontName);
            }

            var allDocumentFonts = GetAllDocumentFonts();
            if (allDocumentFonts.ContainsKey(fontName))
            {
                return PdfFontFactory.CreateFont(allDocumentFonts[fontName]);
            }
        }

        return null;
    }

    List<string>? _allAvailableFonts;
    public List<string> AllAvailableFontsKeys
    {
        get 
        {
            if (_allAvailableFonts == null)
            {
                var allFonts = new List<string>();

                allFonts.AddRange(GetAllDocumentFonts().Keys.Select(f => f.ToLowerInvariant()));
                allFonts.AddRange(_fontsCache.FontsKeys);

                _allAvailableFonts = allFonts.Distinct().ToList();
            }

            return _allAvailableFonts;
        }
    }

    Dictionary<string, PdfDictionary>? _documentFonts;
    Dictionary<string, PdfDictionary> GetAllDocumentFonts()
    {
        if (_documentFonts == null)
        {
            _documentFonts = new Dictionary<string, PdfDictionary>();

            var pagesCount = _pdfDocument.GetNumberOfPages();
            for (int i = 1; i <= pagesCount; i++)
            {
                var page = _pdfDocument.GetPage(i);
                PdfDictionary fontsDict = page.GetResources().GetResource(PdfName.Font);

                if (fontsDict != null)
                {
                    foreach (PdfName key in fontsDict.KeySet())
                    {
                        var fontDict = fontsDict.GetAsDictionary(key);
                        if (fontDict != null)
                        {
                            string? fontName = fontDict.GetAsName(PdfName.BaseFont)?.GetValue().ToLowerInvariant();

                            if (!string.IsNullOrEmpty(fontName) && !_documentFonts.ContainsKey(fontName))
                                _documentFonts.Add(fontName, fontDict);
                        }
                    }
                }
            }
        }

        return _documentFonts;
    }
}

class FontsCache
{
    private static FontsCache? _instance;

    IConfiguration? _configuration;
    Logger _logger;

    private FontsCache(IConfiguration? configuration, Logger logger)
    {
        _configuration = configuration;
        _logger = logger;

        ReadFontsKeys();
    }

    public static FontsCache GetInstance(IConfiguration? configuration, Logger logger)
    {
        if (_instance == null)
            _instance = new FontsCache(configuration, logger);

        return _instance;
    }

    public bool FontExists(string fontName)
    {
        return FontsKeys.Contains(fontName.Trim().ToLowerInvariant());
    }

    public PdfFont? CreateFont(string fontName)
    {
        fontName = fontName.Trim().ToLowerInvariant();

        if (FontExists(fontName))
        {
            if (FontProgramFactory.GetRegisteredFonts().Contains(fontName))
            {
                return PdfFontFactory.CreateRegisteredFont(fontName);
            }

            var allFontsFromResources = GetAllFontsFromResources();
            if (allFontsFromResources.ContainsKey(fontName))
            {
                return PdfFontFactory.CreateFont(allFontsFromResources[fontName]);
            }
        }

        return null;
    }

    public List<string> FontsKeys
    {
        get
        {
            return ReadFontsKeys();
        }
    }

    List<string>? _fontsKeys;
    public List<string> ReadFontsKeys()
    {
        if (_fontsKeys == null)
        {
            var allFonts = new List<string>();

            allFonts.AddRange(FontProgramFactory.GetRegisteredFonts().Select(f => f.ToLowerInvariant()));
            allFonts.AddRange(GetAllFontsFromResources().Keys);

            _fontsKeys = allFonts.Distinct().ToList();
        }

        return _fontsKeys;
    }

    Dictionary<string, string>? _resourcesFonts;
    private Dictionary<string, string> GetAllFontsFromResources()
    {
        if (_resourcesFonts == null)
        {
            _resourcesFonts = new Dictionary<string, string>();

            if (_configuration != null)
            {
                var fontsPaths = _configuration.GetSection("Resources:Fonts");
                if (fontsPaths != null)
                {
                    foreach (var pathSection in fontsPaths.GetChildren())
                    {
                        var path = pathSection.Value;
                        if (!string.IsNullOrEmpty(path))
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(path);
                            if (directoryInfo.Exists)
                            {
                                List<FileInfo> files = directoryInfo.GetFiles().ToList();

                                foreach (var file in files)
                                {
                                    String fileExtension = file.Extension.ToLowerInvariant();
                                    switch (fileExtension)
                                    {
                                        case ".afm":
                                        case ".pfm":
                                            String pfb = IO.Path.ChangeExtension(file.FullName, ".pfb");
                                            if (FileUtil.FileExists(pfb))
                                                AddToFontList(file.FullName);
                                            break;

                                        case ".ttf":
                                        case ".otf":
                                            AddToFontList(file.FullName);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                _logger.Warn($"Fonts directory '{path}' not found.");
                            }
                        }
                    }
                }

                void AddToFontList(string fontPath)
                {
                    FontProgramDescriptor descriptor = FontProgramDescriptorFactory.FetchDescriptor(fontPath);
                    if (descriptor != null)
                    {
                        var fontName = descriptor.GetFontNameLowerCase();
                        if (!_resourcesFonts.ContainsKey(fontName))
                            _resourcesFonts.Add(fontName, fontPath);
                    }
                }
            }
        }

        return _resourcesFonts;
    }
}

