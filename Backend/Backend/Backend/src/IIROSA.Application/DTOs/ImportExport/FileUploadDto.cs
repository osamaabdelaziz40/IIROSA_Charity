using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// File Upload DTO - Used for uploading import files (UC-12.1, UC-12.2)
/// </summary>
public class FileUploadDto
{
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;

    [Required(ErrorMessage = "Correspondence type is required")]
    [StringLength(50, ErrorMessage = "Correspondence type cannot exceed 50 characters")]
    public string CorrespondenceType { get; set; } = string.Empty; // Incoming, Outgoing

    public bool SkipDuplicates { get; set; } = false;
    public bool UpdateExisting { get; set; } = false;
    public bool ValidateOnly { get; set; } = false;

    public string? FieldMapping { get; set; } // JSON
}

/// <summary>
/// Field Mapping DTO - Used for mapping file columns to system fields (UC-12.4)
/// </summary>
public class FieldMappingDto
{
    public string FileColumn { get; set; } = string.Empty;
    public string SystemField { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
}

/// <summary>
/// Validation Result DTO - Used for validation results (UC-12.3, UC-12.5)
/// </summary>
public class ValidationResultDto
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int WarningRows { get; set; }
    public int ErrorRows { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public bool CanProceed { get; set; }
}

/// <summary>
/// Validation Error DTO - Individual error details
/// </summary>
public class ValidationErrorDto
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error"; // Error, Warning
}

/// <summary>
/// Import Preview DTO - Used for preview before commit (UC-12.5)
/// </summary>
public class ImportPreviewDto
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<object> PreviewData { get; set; } = new();
    public ValidationResultDto ValidationResults { get; set; } = new();
    public List<FieldMappingDto> FieldMappings { get; set; } = new();
}
