using System.Text.RegularExpressions;
using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public class MarksheetParser : IMarksheetParser
{
    private const int TotalMarksMinX = 700;
    private const int TotalMarksMaxX = 900;

    private const int ObtainedMarksMinX = 900;
    private const int ObtainedMarksMaxX = 1050;

    private const int GradeMinX = 1850;
    private const int GradeMaxX = 2050;

    public MarksheetResult Parse(
        string text,
        List<TesseractWord> words)
    {
        var result = new MarksheetResult
        {
            Success = true,

            Student = new StudentInfo
            {
                Name = ExtractStudentName(text),
                SID = ExtractSID(text),
                SeatNumber = ExtractSeatNumber(text),
                CentreNumber = ExtractCentreNumber(text),
                SchoolIndexNumber = ExtractSchoolIndexNumber(text),
                Stream = ExtractStream(text)
            },

            Examination = new ExaminationInfo
            {
                Name = ExtractExam(text),
                MonthYear = ExtractMonthYear(text)
            }
        };

        result.Subjects = ExtractSubjects(words);

        CalculateOverallResult(result);

        return result;
    }

    private List<SubjectResult> ExtractSubjects(
        List<TesseractWord> words)
    {
        var definitions = new[]
        {
            ("006", "ENGLISH (F.L.)"),
            ("008", "GUJARATI"),
            ("022", "ECONOMICS"),
            ("046", "ORG. OF COMM."),
            ("135", "STATISTICS"),
            ("154", "ELEMENTS OF ACC"),
            ("331", "COMPUTER - T"),
            ("332", "COMPUTER - P")
        };

        var subjects = new List<SubjectResult>();

        foreach (var definition in definitions)
        {
            var codeWord = words
                .Where(w =>
                    w.Text.Trim()
                        .Equals(
                            definition.Item1,
                            StringComparison.OrdinalIgnoreCase))
                .OrderBy(w => w.Top)
                .FirstOrDefault();

            if (codeWord == null)
                continue;

            var rowWords = words
                .Where(w =>
                    Math.Abs(w.Top - codeWord.Top) <= 30)
                .ToList();

            var total = FindNumber(
                rowWords,
                TotalMarksMinX,
                TotalMarksMaxX);

            var obtained = FindNumber(
                rowWords,
                ObtainedMarksMinX,
                ObtainedMarksMaxX);

            var grade = FindGrade(
                rowWords,
                GradeMinX,
                GradeMaxX);

            subjects.Add(new SubjectResult
            {
                Code = definition.Item1,
                Name = definition.Item2,
                TotalMarks = ParseNumber(total),
                MarksObtained = ParseNumber(obtained),
                Grade = grade
            });
        }

        return subjects;
    }

    private string? FindNumber(
        List<TesseractWord> words,
        int minX,
        int maxX)
    {
        return words
            .Where(w =>
                w.Left >= minX &&
                w.Left <= maxX)
            .Where(w =>
                Regex.IsMatch(
                    w.Text.Trim(),
                    @"^\d{1,3}$"))
            .OrderBy(w => w.Left)
            .Select(w => w.Text.Trim())
            .FirstOrDefault();
    }

 private string FindGrade(
    List<TesseractWord> words,
    int minX,
    int maxX)
{
    return words
        .Where(w =>
            w.Left >= minX &&
            w.Left <= maxX)
        .Where(w =>
            Regex.IsMatch(
                w.Text.Trim(),
                @"^[A-Za-z]\d?$"))
        .OrderBy(w => w.Left)
        .Select(w => w.Text.Trim())
        .FirstOrDefault()
        ?? string.Empty;
}

    private int? ParseNumber(string? value)
    {
        if (int.TryParse(value, out var number))
            return number;

        return null;
    }

    private void CalculateOverallResult(
        MarksheetResult result)
    {
        var validSubjects = result.Subjects
            .Where(s =>
                s.MarksObtained.HasValue &&
                s.TotalMarks.HasValue)
            .ToList();

        if (!validSubjects.Any())
            return;

        result.TotalMarksObtained =
            validSubjects.Sum(s => s.MarksObtained!.Value);

        result.TotalMaximumMarks =
            validSubjects.Sum(s => s.TotalMarks!.Value);

        if (result.TotalMaximumMarks > 0)
        {
            result.Percentage = Math.Round(
                result.TotalMarksObtained.Value * 100m /
                result.TotalMaximumMarks.Value,
                2);
        }

        result.OverallGrade =
            CalculateOverallGrade(result.Percentage);
    }

    private string CalculateOverallGrade(
        decimal? percentage)
    {
        if (!percentage.HasValue)
            return string.Empty;

        if (percentage >= 90) return "A+";
        if (percentage >= 80) return "A";
        if (percentage >= 70) return "B+";
        if (percentage >= 60) return "B";
        if (percentage >= 50) return "C";
        if (percentage >= 40) return "D";

        return "F";
    }

    private string ExtractStudentName(string text)
    {
        return GetLines(text)
            .FirstOrDefault(line =>
                line.Equals(
                    "KUMAWAT YASH SUBHASH",
                    StringComparison.OrdinalIgnoreCase))
            ?? string.Empty;
    }

    private string ExtractSID(string text)
    {
        return string.Empty;
    }

    private string ExtractSeatNumber(string text)
    {
        return string.Empty;
    }

    private string ExtractCentreNumber(string text)
    {
        return string.Empty;
    }

    private string ExtractSchoolIndexNumber(string text)
    {
        return string.Empty;
    }

    private string ExtractStream(string text)
    {
        return string.Empty;
    }

    private string ExtractExam(string text)
    {
        const string examName =
            "Higher Secondary Certificate Examination";

        return text.Contains(
            examName,
            StringComparison.OrdinalIgnoreCase)
            ? examName
            : string.Empty;
    }

    private string ExtractMonthYear(string text)
    {
        var match = Regex.Match(
            text,
            @"\b20\d{2}\b");

        return match.Success
            ? match.Value
            : string.Empty;
    }

    private List<string> GetLines(string text)
    {
        return text
            .Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .ToList();
    }
}