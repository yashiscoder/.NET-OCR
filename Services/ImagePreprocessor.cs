using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace OCR_DotNet.Services;

public class ImagePreprocessor : IImagePreprocessor
{
    public async Task<string> PreprocessAsync(string imagePath)
    {
        using var image = await Image.LoadAsync(imagePath);

        image.Mutate(x =>
        {
            x.Grayscale();

            x.Resize(new ResizeOptions
            {
                Size = new Size(image.Width * 2, image.Height * 2),
                Mode = ResizeMode.Stretch
            });

            x.BinaryThreshold(0.5f);
        });

        var outputPath = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid()}_processed.png"
        );

        await image.SaveAsPngAsync(outputPath);

        return outputPath;
    }
}