namespace OCR_DotNet.Models;

public class TesseractWord
{
    public int Level { get; set; }

    public int PageNum { get; set; }

    public int BlockNum { get; set; }

    public int ParNum { get; set; }

    public int LineNum { get; set; }

    public int WordNum { get; set; }

    public int Left { get; set; }

    public int Top { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public float Confidence { get; set; }

    public string Text { get; set; } = string.Empty;

    public int Right => Left + Width;

public int Bottom => Top + Height;
}