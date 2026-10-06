namespace OCR_DotNet.Models;

public class OcrResult
{
    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string Text { get; set; } = string.Empty;
}