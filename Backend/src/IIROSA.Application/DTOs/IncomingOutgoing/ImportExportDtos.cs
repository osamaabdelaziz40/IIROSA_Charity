namespace IIROSA.Application.DTOs.IncomingOutgoing;

/// <summary>
/// Import Request DTO - Base
/// </summary>
public class ImportRequestDto
{
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public Dictionary<string, string> ColumnMappings { get; set; } = new();
    public bool SkipDuplicates { get; set; } = true;
    public bool UpdateExisting { get; set; } = false;
    public bool ValidateOnly { get; set; } = false;
}

/// <summary>
/// Import Incoming Letters Request DTO
/// </summary>
public class ImportIncomingRequestDto : ImportRequestDto
{
}

/// <summary>
/// Import Outgoing Letters Request DTO
/// </summary>
public class ImportOutgoingRequestDto : ImportRequestDto
{
}

/// <summary>
/// Import Validation Result DTO
/// </summary>
public class ImportValidationResultDto
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<ValidationWarningDto> Warnings { get; set; } = new();
}

/// <summary>
/// Validation Error DTO
/// </summary>
public class ValidationErrorDto
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error"; // Error, Warning
}

/// <summary>
/// Validation Warning DTO
/// </summary>
public class ValidationWarningDto
{
    public int RowNumber { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string WarningMessage { get; set; } = string.Empty;
}

/// <summary>
/// Import Result DTO
/// </summary>
public class ImportResultDto
{
    public Guid ImportId { get; set; }
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public List<ValidationErrorDto> Errors { get; set; } = new();
    public List<Guid> ImportedRecordIds { get; set; } = new();
}

/// <summary>
/// Export Request DTO - Base
/// </summary>
public class ExportRequestDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DepartmentId { get; set; }
    public int? Year { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public List<string> FieldsToExport { get; set; } = new();
    public string FileFormat { get; set; } = "Excel"; // Excel, PDF, CSV
    public string Language { get; set; } = "Both"; // Arabic, English, Both
    public bool IncludeAttachments { get; set; } = false;
    public string SortBy { get; set; } = "Date";
    public string SortOrder { get; set; } = "Descending"; // Ascending, Descending
}

/// <summary>
/// Export Incoming Letters Request DTO
/// </summary>
public class ExportIncomingRequestDto : ExportRequestDto
{
    public string? Status { get; set; }
}

/// <summary>
/// Export Outgoing Letters Request DTO
/// </summary>
public class ExportOutgoingRequestDto : ExportRequestDto
{
    public int? CategoryId { get; set; }
    public bool? HasReply { get; set; }
}

/// <summary>
/// Export Result DTO
/// </summary>
public class ExportResultDto
{
    public Guid ExportId { get; set; }
    public int RecordCount { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
    public DateTime ExportDate { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public List<string> FiltersApplied { get; set; } = new();
    public List<string> FieldsExported { get; set; } = new();
}

/// <summary>
/// Import History Item DTO
/// </summary>
public class ImportHistoryItemDto
{
    public Guid ImportId { get; set; }
    public string ImportType { get; set; } = string.Empty; // Incoming Letters, Outgoing Letters
    public string FileName { get; set; } = string.Empty;
    public DateTime ImportDate { get; set; }
    public string ImportedBy { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = string.Empty; // Success, Partial Success, Failed, Rolled Back
}

/// <summary>
/// Export History Item DTO
/// </summary>
public class ExportHistoryItemDto
{
    public Guid ExportId { get; set; }
    public string ExportType { get; set; } = string.Empty; // Incoming Letters, Outgoing Letters
    public DateTime ExportDate { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public int RecordCount { get; set; }
    public string FileFormat { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public List<string> FiltersApplied { get; set; } = new();
}

/// <summary>
/// Column Mapping DTO
/// </summary>
public class ColumnMappingDto
{
    public string FileColumn { get; set; } = string.Empty;
    public string SystemField { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public string? DataType { get; set; } // string, int, DateTime, bool, etc.
}

/// <summary>
/// Template Download DTO
/// </summary>
public class TemplateDownloadDto
{
    public string TemplateType { get; set; } = string.Empty; // IncomingLetters, OutgoingLetters
    public string FileName { get; set; } = string.Empty;
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
}
