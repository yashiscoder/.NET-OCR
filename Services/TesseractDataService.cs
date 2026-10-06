using System.Diagnostics;
using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public class TesseractDataService : ITesseractDataService
{
    public async Task<List<TesseractWord>> ExtractWordsAsync(
        string imagePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/tesseract",
            Arguments = $"\"{imagePath}\" stdout --psm 6 tsv",
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
                $"Tesseract TSV failed: {error}");
        }

        return ParseTsv(output);
    }

    private List<TesseractWord> ParseTsv(string tsv)
    {
        var words = new List<TesseractWord>();

        var lines = tsv.Split(
            '\n',
            StringSplitOptions.RemoveEmptyEntries);

        // Skip TSV header
        foreach (var line in lines.Skip(1))
        {
            var columns = line.Split('\t');

            if (columns.Length < 12)
            {
                continue;
            }

            // Empty text means Tesseract detected
            // a structural element rather than a word.
            if (string.IsNullOrWhiteSpace(columns[11]))
            {
                continue;
            }

            if (!int.TryParse(columns[0], out var level))
                continue;

            if (!int.TryParse(columns[1], out var pageNum))
                continue;

            if (!int.TryParse(columns[2], out var blockNum))
                continue;

            if (!int.TryParse(columns[3], out var parNum))
                continue;

            if (!int.TryParse(columns[4], out var lineNum))
                continue;

            if (!int.TryParse(columns[5], out var wordNum))
                continue;

            if (!int.TryParse(columns[6], out var left))
                continue;

            if (!int.TryParse(columns[7], out var top))
                continue;

            if (!int.TryParse(columns[8], out var width))
                continue;

            if (!int.TryParse(columns[9], out var height))
                continue;

            if (!float.TryParse(
                columns[10],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var confidence))
            {
                continue;
            }

            words.Add(new TesseractWord
            {
                Level = level,
                PageNum = pageNum,
                BlockNum = blockNum,
                ParNum = parNum,
                LineNum = lineNum,
                WordNum = wordNum,
                Left = left,
                Top = top,
                Width = width,
                Height = height,
                Confidence = confidence,
                Text = columns[11].Trim()
            });
        }

        return words;
    }
    private List<MarksheetRow> GroupIntoRows(
    List<TesseractWord> words)
{
    var rows = new List<MarksheetRow>();

    foreach (var word in words.OrderBy(w => w.Top))
    {
        var row = rows.FirstOrDefault(r =>
            Math.Abs(r.Top - word.Top) <= 25);

        if (row == null)
        {
            row = new MarksheetRow
            {
                Top = word.Top
            };

            rows.Add(row);
        }

        row.Words.Add(word);
    }

    foreach (var row in rows)
    {
        row.Words = row.Words
            .OrderBy(w => w.Left)
            .ToList();
    }

    return rows;
}
}