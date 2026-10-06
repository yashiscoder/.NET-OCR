using Microsoft.AspNetCore.Mvc;
using OCR_DotNet.Services;

namespace OCR_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OcrController : ControllerBase
{
    private readonly IOcrService _ocrService;
    private readonly IImagePreprocessor _imagePreprocessor;

    public OcrController(
        IOcrService ocrService,
        IImagePreprocessor imagePreprocessor)
    {
        _ocrService = ocrService;
        _imagePreprocessor = imagePreprocessor;
    }

    [HttpPost]
    public async Task<IActionResult> ExtractText(IFormFile file)
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
                "Only JPG, JPEG, and PNG images are allowed."
            );
        }

        const long maxFileSize = 5 * 1024 * 1024;

        if (file.Length > maxFileSize)
        {
            return BadRequest(
                "File size must not exceed 5 MB."
            );
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
                    filePath
                );

            var result =
                await _ocrService.ExtractTextAsync(
                    processedFilePath,
                    file.FileName,
                    file.Length
                );

            return Ok(result);
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