using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Create Export Log DTO - Used for logging export operations (UC-12.10, UC-12.11)
/// </summary>
public class CreateExportLogDto
{
    [Required(ErrorMessage = "Correspondence type is required")]
    [StringLength(50, ErrorMessage = "Correspondence type cannot exceed 50 characters")]
    public string CorrespondenceType { get; set; } = string.Empty; // Incoming, Outgoing

    [Required(ErrorMessage = "File name is required")]
    [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
    public string FileName { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    [StringLength(50, ErrorMessage = "Export format cannot exceed 50 characters")]
    public string? ExportFormat { get; set; } // Excel, PDF, CSV

    public string? SelectedFields { get; set; } // JSON
    public string? AppliedFilters { get; set; } // JSON

    public int TotalRows { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = "Success"; // Success, Failed

    public string? ErrorLog { get; set; }
    public string? Notes { get; set; }
}
