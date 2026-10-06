namespace OCR_DotNet.Models;

public class SubjectResult
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? MarksObtained { get; set; }

    public string? TotalMarks { get; set; }
}