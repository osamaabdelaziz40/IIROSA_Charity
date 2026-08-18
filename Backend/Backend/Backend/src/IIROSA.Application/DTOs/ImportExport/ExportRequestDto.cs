using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Export Request DTO - Used for exporting correspondence (UC-12.10, UC-12.11)
/// </summary>
public class ExportRequestDto
{
    [Required(ErrorMessage = "Correspondence type is required")]
    [StringLength(50, ErrorMessage = "Correspondence type cannot exceed 50 characters")]
    public string CorrespondenceType { get; set; } = string.Empty; // Incoming, Outgoing

    // Filters (UC-12.13)
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public Guid? CreatedByUserId { get; set; }

    // Fields to Export (UC-12.12)
    public List<string> SelectedFields { get; set; } = new();

    // Format Options
    [Required(ErrorMessage = "Export format is required")]
    [StringLength(50, ErrorMessage = "Export format cannot exceed 50 characters")]
    public string ExportFormat { get; set; } = "Excel"; // Excel, PDF, CSV

    public string Language { get; set; } = "Both"; // Arabic, English, Both

    // Sort Options
    public string SortBy { get; set; } = "Date";
    public bool SortDescending { get; set; } = false;
}

/// <summary>
/// Rollback Import DTO - Used for rolling back imports (UC-12.8)
/// </summary>
public class RollbackImportDto
{
    [Required(ErrorMessage = "Import log ID is required")]
    public Guid ImportLogId { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Import Commit DTO - Used for committing import after preview (UC-12.6)
/// </summary>
public class ImportCommitDto
{
    [Required(ErrorMessage = "Import log ID is required")]
    public Guid ImportLogId { get; set; }

    public bool ProceedWithValidRowsOnly { get; set; } = false;
}
