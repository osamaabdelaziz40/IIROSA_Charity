namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Import/Export Log DTO - Full details for single log entry
/// </summary>
public class ImportExportLogDto
{
    public Guid Id { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string CorrespondenceType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public DateTime OperationDate { get; set; }
    public string? OperatedBy { get; set; }
    public Guid? OperatedByUserId { get; set; }
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImportType { get; set; }
    public bool SkipDuplicates { get; set; }
    public bool UpdateExisting { get; set; }
    public bool ValidateOnly { get; set; }
    public string? FieldMapping { get; set; }
    public string? ValidationErrors { get; set; }
    public string? ExportFormat { get; set; }
    public string? SelectedFields { get; set; }
    public string? AppliedFilters { get; set; }
    public DateTime? RollbackDate { get; set; }
    public string? RollbackBy { get; set; }
    public string? RollbackNotes { get; set; }
    public string? ErrorLog { get; set; }
    public string? Notes { get; set; }
    public bool IsSuccessful { get; set; }
    public bool IsPartiallySuccessful { get; set; }
    public bool IsRolledBack { get; set; }
    public double SuccessRate { get; set; }
    public DateTime CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
}
