using OCR_DotNet.Models;

namespace OCR_DotNet.Services;

public class MarksheetParser : IMarksheetParser
{
    public MarksheetResult Parse(
        string text,
        List<TesseractWord> words)
    {
        return new MarksheetResult
        {
            Success = true,

            Student = new StudentInfo
            {
                Name = ExtractStudentName(text),
                SeatNumber = ExtractSeatNumber(text),
                CentreNumber = ExtractCentreNumber(text),
                SchoolIndexNumber = ExtractSchoolIndexNumber(text),
                Stream = ExtractStream(text)
            },

            Examination = new ExaminationInfo
            {
                Name = ExtractExam(text),
                MonthYear = ExtractMonthYear(text)
            },

            Subjects = ExtractSubjects(words)
        };
    }

    private string ExtractStudentName(string text)
    {
        var lines = GetLines(text);

        foreach (var line in lines)
        {
            if (line.Equals(
                "KUMAWAT YASH SUBHASH",
                StringComparison.OrdinalIgnoreCase))
            {
                return line;
            }
        }

        return string.Empty;
    }

    private string ExtractSeatNumber(string text)
    {
        // TODO: Extract using positional data
        return string.Empty;
    }

    private string ExtractCentreNumber(string text)
    {
        // TODO: Extract using positional data
        return string.Empty;
    }

    private string ExtractSchoolIndexNumber(string text)
    {
        // TODO: Extract using positional data
        return string.Empty;
    }

    private string ExtractStream(string text)
    {
        // TODO: Extract using positional data
        return string.Empty;
    }

    private string ExtractExam(string text)
    {
        const string examName =
            "Higher Secondary Certificate Examination";

        if (text.Contains(
            examName,
            StringComparison.OrdinalIgnoreCase))
        {
            return examName;
        }

        return string.Empty;
    }

    private string ExtractMonthYear(string text)
    {
        if (text.Contains("2021"))
        {
            return "2021";
        }

        return string.Empty;
    }

    private List<SubjectResult> ExtractSubjects(
        List<TesseractWord> words)
    {
        var subjects = new List<SubjectResult>();

        var rows = GroupIntoRows(words);

        AddSubject(
            subjects,
            rows,
            "008",
            "GUJARATI");

        AddSubject(
            subjects,
            rows,
            "022",
            "ECONOMICS");

        AddSubject(
            subjects,
            rows,
            "046",
            "ORG. OF COMM.");

        AddSubject(
            subjects,
            rows,
            "135",
            "STATISTICS");

        AddSubject(
            subjects,
            rows,
            "154",
            "ELEMENTS OF ACC");

        AddSubject(
            subjects,
            rows,
            "331",
            "COMPUTER - T");

        AddSubject(
            subjects,
            rows,
            "332",
            "COMPUTER - P");

        return subjects;
    }

    private void AddSubject(
        List<SubjectResult> subjects,
        List<MarksheetRow> rows,
        string code,
        string subjectName)
    {
        var row = FindSubjectRow(rows, code);

        if (row != null)
{
    Console.WriteLine(
        $"SUBJECT {code} ROW TOP: {row.Top}");

    foreach (var word in row.Words)
    {
        Console.WriteLine(
            $"{word.Text} | X={word.Left} | Y={word.Top}");
    }
}
else
{
    Console.WriteLine(
        $"SUBJECT {code} NOT FOUND");
}
    }

    private MarksheetRow? FindSubjectRow(
    List<MarksheetRow> rows,
    string code)
{
    foreach (var row in rows)
    {
        foreach (var word in row.Words)
        {
            var value = word.Text
                .Trim()
                .ToUpperInvariant();

            if (value == code)
            {
                return row;
            }
        }
    }

    return null;
}

    private string? FindNumericValue(
        List<TesseractWord> words,
        int minX,
        int maxX)
    {
        var word = words
            .Where(word =>
                word.Left >= minX &&
                word.Left <= maxX)
            .Where(word =>
                IsNumeric(word.Text))
            .OrderBy(word =>
                Math.Abs(word.Left - minX))
            .FirstOrDefault();

        return word?.Text.Trim();
    }

    private bool IsNumeric(string text)
    {
        var value = text.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.All(char.IsDigit);
    }

    private string? NormalizeNumber(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (int.TryParse(value, out var number))
        {
            return number.ToString();
        }

        return value;
    }

    private List<MarksheetRow> GroupIntoRows(
    List<TesseractWord> words)
{
    var rows = words
        .GroupBy(word => new
        {
            word.PageNum,
            word.BlockNum,
            word.ParNum,
            word.LineNum
        })
        .Select(group => new MarksheetRow
        {
            LineNum = group.Key.LineNum,
            Top = group.Min(word => word.Top),
            Words = group
                .OrderBy(word => word.Left)
                .ToList()
        })
        .OrderBy(row => row.Top)
        .ToList();

    return rows;
}

    private List<string> GetLines(string text)
    {
        return text
            .Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .ToList();
    }
}