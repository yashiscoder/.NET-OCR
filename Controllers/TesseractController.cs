using Microsoft.AspNetCore.Mvc;
using OCR_DotNet.Services;

namespace OCR_DotNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TesseractController : ControllerBase
{
    private readonly ITesseractDataService _tesseractDataService;

    public TesseractController(
        ITesseractDataService tesseractDataService)
    {
        _tesseractDataService = tesseractDataService;
    }

    [HttpPost("words")]
    public async Task<IActionResult> GetWords(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please upload an image.");
        }

        var extension = Path
            .GetExtension(file.FileName)
            .ToLowerInvariant();

        var filePath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}{extension}");

        try
        {
            await using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var words =
                await _tesseractDataService
                    .ExtractWordsAsync(filePath);

            return Ok(words);
        }
        finally
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}