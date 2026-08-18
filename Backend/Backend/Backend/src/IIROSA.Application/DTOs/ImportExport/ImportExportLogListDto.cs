namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Import/Export Log List DTO - Used for grid/list views (UC-12.7, UC-12.14)
/// </summary>
public class ImportExportLogListDto
{
    public Guid Id { get; set; }
    public string OperationType { get; set; } = string.Empty; // Import, Export
    public string CorrespondenceType { get; set; } = string.Empty; // Incoming, Outgoing
    public string FileName { get; set; } = string.Empty;
    public DateTime OperationDate { get; set; }
    public string? OperatedBy { get; set; }
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImportType { get; set; }
    public string? ExportFormat { get; set; }
    public double SuccessRate { get; set; }
    public bool IsRolledBack { get; set; }
}
