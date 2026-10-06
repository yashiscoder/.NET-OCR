using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public interface IMarksheetParser
{
    MarksheetResult Parse(
        string text,
        List<TesseractWord> words);
}