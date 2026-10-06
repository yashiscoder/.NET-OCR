namespace OCR_DotNet.Services;

public interface IImagePreprocessor
{
    Task<string> PreprocessAsync(string imagePath);
}