using System.Diagnostics;
using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public class TesseractOcrService : IOcrService
{
    public async Task<OcrResult> ExtractTextAsync(
        string imagePath,
        string fileName,
        long fileSize)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/tesseract",
            Arguments = $"\"{imagePath}\" stdout",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process();

        process.StartInfo = startInfo;

        process.Start();

        var output =
            await process.StandardOutput.ReadToEndAsync();

        var error =
            await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"Tesseract failed: {error}");
        }

        return new OcrResult
        {
            FileName = fileName,
            FileSize = fileSize,
            Text = output
        };
    }
}