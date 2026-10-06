using OCR_DotNet.Models;

namespace OCR_DotNet.Models;

public class MarksheetRow
{
    public int LineNum { get; set; }

    public int Top { get; set; }

    public List<TesseractWord> Words { get; set; } = new();
}