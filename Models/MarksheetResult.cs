namespace OCR_DotNet.Models;

public class MarksheetResult
{
    public bool Success { get; set; }

    public StudentInfo Student { get; set; } = new();

    public ExaminationInfo Examination { get; set; } = new();

    public List<SubjectResult> Subjects { get; set; } = new();

    public int? TotalMarksObtained { get; set; }

    public int? TotalMaximumMarks { get; set; }

    public decimal? Percentage { get; set; }

    public string OverallGrade { get; set; } = string.Empty;
}