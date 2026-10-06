using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public interface IOcrService
{
    Task<OcrResult> ExtractTextAsync(
        string imagePath,
        string fileName,
        long fileSize);
}