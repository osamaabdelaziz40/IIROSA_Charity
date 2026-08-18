using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Import/Export Log entity - Tracks all import and export operations for correspondence
/// Implements all use cases UC-12.1 through UC-12.14
/// Inherits from FullAuditedEntityBase<Guid> with all audit fields
/// </summary>
public class ImportExportLog : FullAuditedEntity
{
    // Operation Information
    public string OperationType { get; set; } = string.Empty; // Import, Export
    public string CorrespondenceType { get; set; } = string.Empty; // Incoming, Outgoing
    public string FileName { get; set; } = string.Empty;
    public string? FilePath { get; set; }

    // Operation Details
    public DateTime OperationDate { get; set; } = DateTime.UtcNow;
    public string? OperatedBy { get; set; } // User name
    public Guid? OperatedByUserId { get; set; }

    // Import/Export Results
    public int TotalRows { get; set; }
    public int SuccessfulRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Success, PartialSuccess, Failed, RolledBack

    // Import Specific (UC-12.1, UC-12.2, UC-12.6)
    public string? ImportType { get; set; } // IncomingLetters, OutgoingLetters
    public bool SkipDuplicates { get; set; } = false;
    public bool UpdateExisting { get; set; } = false;
    public bool ValidateOnly { get; set; } = false;
    public string? FieldMapping { get; set; } // JSON serialization of field mappings
    public string? ValidationErrors { get; set; } // JSON serialization of validation errors

    // Export Specific (UC-12.10, UC-12.11, UC-12.12, UC-12.13)
    public string? ExportFormat { get; set; } // Excel, PDF, CSV
    public string? SelectedFields { get; set; } // JSON serialization of selected fields
    public string? AppliedFilters { get; set; } // JSON serialization of applied filters

    // Rollback Information (UC-12.8)
    public DateTime? RollbackDate { get; set; }
    public string? RollbackBy { get; set; }
    public Guid? RollbackByUserId { get; set; }
    public string? RollbackNotes { get; set; }
    public string? DeletedRecordIds { get; set; } // JSON array of IDs that were deleted during rollback

    // Error Details
    public string? ErrorLog { get; set; } // Detailed error log
    public string? Notes { get; set; }

    // Calculated properties
    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsSuccessful => Status == "Success";

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsPartiallySuccessful => Status == "PartialSuccess";

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsRolledBack => Status == "RolledBack";

    [System.Text.Json.Serialization.JsonIgnore]
    public double SuccessRate => TotalRows > 0 ? (double)SuccessfulRows / TotalRows * 100 : 0;
}
