using System;
using System.Collections.Generic;

namespace IIROSA.Application.DTOs.AuditLog
{
    #region Main DTOs

    /// <summary>
    /// Base DTO for audit log entries
    /// </summary>
    public class AuditLogDto
    {
        public Guid AuditLogId { get; set; }
        public string EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public string Operation { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
        public string FieldChanges { get; set; }
        public string AdditionalContext { get; set; }
        public Guid? CorrelationId { get; set; }
        public string UserAgent { get; set; }
    }

    /// <summary>
    /// Lightweight DTO for audit log list views
    /// </summary>
    public class AuditLogListDto
    {
        public Guid AuditLogId { get; set; }
        public string EntityType { get; set; }
        public string Operation { get; set; }
        public string OperationDisplayName { get; set; }
        public string UserName { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public Guid? EntityId { get; set; }
        public string Summary { get; set; }
    }

    /// <summary>
    /// Detailed DTO including parsed field changes
    /// </summary>
    public class AuditLogDetailDto : AuditLogDto
    {
        public List<FieldChangeDto> ParsedFieldChanges { get; set; }
        public Dictionary<string, object> ParsedContext { get; set; }
        public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm:ss UTC");
        public string OperationDisplayName
        {
            get
            {
                return Operation switch
                {
                    "Create" => "Created",
                    "Update" => "Updated",
                    "Delete" => "Deleted",
                    "Login" => "Logged In",
                    "Logout" => "Logged Out",
                    "FailedLogin" => "Failed Login",
                    "Export" => "Exported",
                    "Restore" => "Restored",
                    _ => Operation
                };
            }
        }
    }

    #endregion

    #region Filter and Search DTOs

    /// <summary>
    /// Filter criteria for querying audit logs
    /// </summary>
    public class AuditLogFilterDto
    {
        /// <summary>
        /// Filter by entity type
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Filter by operation type
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Filter by specific user ID
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Start of date range (UTC)
        /// </summary>
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// End of date range (UTC)
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Filter by specific entity ID
        /// </summary>
        public Guid? EntityId { get; set; }

        /// <summary>
        /// Filter by IP address
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Filter by correlation ID
        /// </summary>
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// Page number for pagination (1-based)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Sort field (e.g., "Timestamp", "EntityType", "Operation", "UserName")
        /// </summary>
        public string SortBy { get; set; } = "Timestamp";

        /// <summary>
        /// Sort direction ("asc" or "desc")
        /// </summary>
        public string SortDirection { get; set; } = "desc";
    }

    /// <summary>
    /// Search criteria for keyword search in audit logs
    /// </summary>
    public class AuditLogSearchDto
    {
        /// <summary>
        /// Keyword to search for in field values, usernames, etc.
        /// </summary>
        public string Keyword { get; set; }

        /// <summary>
        /// Optional entity type filter
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Optional operation filter
        /// </summary>
        public string Operation { get; set; }

        /// <summary>
        /// Optional date range filter
        /// </summary>
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Pagination settings
        /// </summary>
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    #endregion

    #region Entity History DTOs

    /// <summary>
    /// Change history for a specific entity
    /// </summary>
    public class EntityHistoryDto
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; }
        public List<AuditLogDetailDto> HistoryEntries { get; set; }
        public int TotalChanges { get; set; }
        public DateTime FirstChange { get; set; }
        public DateTime LastChange { get; set; }
    }

    /// <summary>
    /// Timeline entry for entity history
    /// </summary>
    public class EntityHistoryTimelineDto
    {
        public Guid AuditLogId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Operation { get; set; }
        public string UserName { get; set; }
        public List<string> ChangedFields { get; set; }
        public string Summary { get; set; }
        public string IpAddress { get; set; }
    }

    #endregion

    #region Field Change DTOs

    /// <summary>
    /// Represents a single field change
    /// </summary>
    public class FieldChangeDto
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public ChangeType ChangeType
        {
            get
            {
                if (OldValue == null && NewValue != null) return ChangeType.Added;
                if (OldValue != null && NewValue == null) return ChangeType.Removed;
                if (OldValue != null && NewValue != null && OldValue != NewValue) return ChangeType.Modified;
                return ChangeType.Unchanged;
            }
        }
    }

    /// <summary>
    /// Type of change for a field
    /// </summary>
    public enum ChangeType
    {
        Added,
        Removed,
        Modified,
        Unchanged
    }

    /// <summary>
    /// Comparison result for two versions of a record
    /// </summary>
    public class RecordVersionComparisonDto
    {
        public Guid BeforeAuditLogId { get; set; }
        public Guid AfterAuditLogId { get; set; }
        public DateTime BeforeTimestamp { get; set; }
        public DateTime AfterTimestamp { get; set; }
        public List<FieldComparisonDto> FieldComparisons { get; set; }
    }

    /// <summary>
    /// Comparison of a single field between two versions
    /// </summary>
    public class FieldComparisonDto
    {
        public string FieldName { get; set; }
        public string BeforeValue { get; set; }
        public string AfterValue { get; set; }
        public bool HasChanged { get; set; }
        public ChangeType ChangeType { get; set; }
    }

    #endregion

    #region User Activity DTOs

    /// <summary>
    /// Complete user activity log
    /// </summary>
    public class UserActivityLogDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public List<AuditLogDetailDto> Activities { get; set; }
        public UserActivitySummaryDto Summary { get; set; }
    }

    /// <summary>
    /// Summary statistics for user activity
    /// </summary>
    public class UserActivitySummaryDto
    {
        public int TotalActions { get; set; }
        public Dictionary<string, int> ActionsByType { get; set; }
        public Dictionary<string, int> ActionsByEntity { get; set; }
        public DateTime FirstActivityDate { get; set; }
        public DateTime LastActivityDate { get; set; }
        public int UniqueEntitiesAffected { get; set; }
        public int UniqueIpAddresses { get; set; }
    }

    /// <summary>
    /// Filter for user activity queries
    /// </summary>
    public class UserActivityFilterDto
    {
        public Guid UserId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Operation { get; set; }
        public string EntityType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    #endregion

    #region Export DTOs

    /// <summary>
    /// Export request for audit logs
    /// </summary>
    public class AuditLogExportRequestDto
    {
        /// <summary>
        /// Required date range for export
        /// </summary>
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }

        /// <summary>
        /// Optional filters
        /// </summary>
        public string EntityType { get; set; }
        public string Operation { get; set; }
        public Guid? UserId { get; set; }

        /// <summary>
        /// Export format: Excel, CSV, or JSON
        /// </summary>
        public ExportFormat Format { get; set; } = ExportFormat.Excel;

        /// <summary>
        /// Include detailed field changes (increases file size)
        /// </summary>
        public bool IncludeFieldChanges { get; set; } = false;

        /// <summary>
        /// Include additional context
        /// </summary>
        public bool IncludeContext { get; set; } = false;
    }

    /// <summary>
    /// Supported export formats
    /// </summary>
    public enum ExportFormat
    {
        Excel,
        Csv,
        Json
    }

    /// <summary>
    /// Result of audit log export
    /// </summary>
    public class AuditLogExportResultDto
    {
        public byte[] FileContents { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int RecordCount { get; set; }
        public DateTime ExportGeneratedAt { get; set; }
        public Guid ExportAuditLogId { get; set; }
    }

    #endregion

    #region Restore DTOs

    /// <summary>
    /// Request to restore an entity to a previous version
    /// </summary>
    public class RestoreVersionRequestDto
    {
        /// <summary>
        /// The audit log ID representing the version to restore
        /// </summary>
        public Guid AuditLogId { get; set; }

        /// <summary>
        /// Optional reason for the restore
        /// </summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// Preview of what will be restored
    /// </summary>
    public class RestorePreviewDto
    {
        public Guid AuditLogId { get; set; }
        public DateTime VersionTimestamp { get; set; }
        public string EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public List<FieldChangeDto> ChangesToApply { get; set; }
        public string Warning { get; set; }
    }

    /// <summary>
    /// Result of a restore operation
    /// </summary>
    public class RestoreResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Guid NewAuditLogId { get; set; }
        public Guid? RestoredEntityId { get; set; }
        public List<string> Warnings { get; set; }
    }

    #endregion

    #region Pagination Response

    /// <summary>
    /// Paginated response for audit log queries
    /// </summary>
    public class PagedAuditLogResponseDto<T>
    {
        public List<T> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }

    #endregion
}
