namespace OCR_DotNet.Models;

public class SubjectResult
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public int? TotalMarks { get; set; }

    public int? MarksObtained { get; set; }

    public string Grade { get; set; } = string.Empty;
}