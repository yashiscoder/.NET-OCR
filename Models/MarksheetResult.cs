namespace OCR_DotNet.Models;

public class MarksheetResult
{
    public bool Success { get; set; }

    public StudentInfo Student { get; set; } = new();

    public ExaminationInfo Examination { get; set; } = new();

    public List<SubjectResult> Subjects { get; set; } = new();
}