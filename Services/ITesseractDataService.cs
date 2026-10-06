using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public interface ITesseractDataService
{
    Task<List<TesseractWord>> ExtractWordsAsync(
        string imagePath);
}