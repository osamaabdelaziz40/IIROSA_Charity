using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Create Import Log DTO - Used for logging import operations (UC-12.1, UC-12.2)
/// </summary>
public class CreateImportLogDto
{
    [Required(ErrorMessage = "Correspondence type is required")]
    [StringLength(50, ErrorMessage = "Correspondence type cannot exceed 50 characters")]
    public string CorrespondenceType { get; set; } = string.Empty; // Incoming, Outgoing

    [Required(ErrorMessage = "File name is required")]
    [StringLength(255, ErrorMessage = "File name cannot exceed 255 characters")]
    public string FileName { get; set; } = string.Empty;

    public string? FilePath { get; set; }

    [Required(ErrorMessage = "Import type is required")]
    [StringLength(50, ErrorMessage = "Import type cannot exceed 50 characters")]
    public string ImportType { get; set; } = string.Empty; // IncomingLetters, OutgoingLetters

    public bool SkipDuplicates { get; set; } = false;
    public bool UpdateExisting { get; set; } = false;
    public bool ValidateOnly { get; set; } = false;

    public string? FieldMapping { get; set; } // JSON
    public string? ValidationErrors { get; set; } // JSON

    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Success, PartialSuccess, Failed

    public string? ErrorLog { get; set; }
    public string? Notes { get; set; }
}
