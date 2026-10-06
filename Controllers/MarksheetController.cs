using Microsoft.AspNetCore.Mvc;
using OCR_DotNet.Services;

namespace OCR_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarksheetController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly ITesseractDataService _tesseractDataService;
    private readonly IImagePreprocessor _imagePreprocessor;
    private readonly IMarksheetParser _marksheetParser;

    public MarksheetController(
        ITesseractDataService tesseractDataService,
        IOcrService ocrService,
        IImagePreprocessor imagePreprocessor,
        IMarksheetParser marksheetParser)
    {
        _tesseractDataService = tesseractDataService;
        _ocrService = ocrService;
        _imagePreprocessor = imagePreprocessor;
        _marksheetParser = marksheetParser;
    }

    [HttpPost]
    public async Task<IActionResult> ExtractMarksheet(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload an image.");
        }

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(
                "Only JPG, JPEG, and PNG images are allowed.");
        }

        const long maxFileSize = 5 * 1024 * 1024;

        if (file.Length > maxFileSize)
        {
            return BadRequest(
                "File size must not exceed 5 MB.");
        }

        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}{extension}"
        );

        string? processedFilePath = null;

        try
        {
            await using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            processedFilePath =
                await _imagePreprocessor.PreprocessAsync(
                    filePath);

            var ocrResult =
                await _ocrService.ExtractTextAsync(
                    processedFilePath,
                    file.FileName,
                    file.Length);
            
            var words =
    await _tesseractDataService
        .ExtractWordsAsync(processedFilePath);

            var marksheet =
    _marksheetParser.Parse(
        ocrResult.Text,
        words);

            return Ok(marksheet);
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            if (processedFilePath != null &&
                System.IO.File.Exists(processedFilePath))
            {
                System.IO.File.Delete(processedFilePath);
            }
        }
    }
}