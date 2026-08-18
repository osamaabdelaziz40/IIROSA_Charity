using IIROSA.Application.DTOs.AuditLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIROSA.Application.Interfaces
{
    /// <summary>
    /// Repository interface for AuditLog entity
    /// </summary>
    public interface IAuditLogRepository
    {
        #region Create Operations

        /// <summary>
        /// Add a new audit log entry
        /// </summary>
        Task AddAsync(AuditLogEntry entry);

        /// <summary>
        /// Add multiple audit log entries in a batch
        /// </summary>
        Task AddRangeAsync(IEnumerable<AuditLogEntry> entries);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get audit log by ID
        /// </summary>
        Task<AuditLogEntry> GetByIdAsync(Guid auditLogId);

        /// <summary>
        /// Get audit logs for a specific entity
        /// </summary>
        Task<List<AuditLogEntry>> GetByEntityIdAsync(Guid entityId, string entityType);

        /// <summary>
        /// Get audit logs for a specific user
        /// </summary>
        Task<List<AuditLogEntry>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Get filtered audit logs with pagination
        /// </summary>
        Task<PagedAuditLogResultDto> GetFilteredAsync(AuditLogFilterDto filter);

        /// <summary>
        /// Search audit logs by keyword
        /// </summary>
        Task<PagedAuditLogResultDto> SearchAsync(AuditLogSearchDto search);

        /// <summary>
        /// Get entity history with full details
        /// </summary>
        Task<EntityHistoryDto> GetEntityHistoryAsync(Guid entityId, string entityType);

        /// <summary>
        /// Get user activity summary
        /// </summary>
        Task<UserActivitySummaryDto> GetUserActivitySummaryAsync(Guid userId, DateTime? dateFrom = null, DateTime? dateTo = null);

        /// <summary>
        /// Get user activity log with pagination
        /// </summary>
        Task<PagedAuditLogResultDto> GetUserActivityAsync(UserActivityFilterDto filter);

        #endregion

        #region Export Operations

        /// <summary>
        /// Get audit logs for export
        /// </summary>
        Task<List<AuditLogEntry>> GetForExportAsync(AuditLogExportRequestDto request);

        #endregion

        #region Comparison Operations

        /// <summary>
        /// Get two audit log entries for comparison
        /// </summary>
        Task<(AuditLogEntry before, AuditLogEntry after)> GetForComparisonAsync(Guid beforeAuditLogId, Guid afterAuditLogId);

        /// <summary>
        /// Get audit log entry for restore preview
        /// </summary>
        Task<AuditLogEntry> GetForRestoreAsync(Guid auditLogId);

        #endregion
    }

    #region Supporting Types

    /// <summary>
    /// Audit log entry data structure (matches the AuditLog entity)
    /// </summary>
    public class AuditLogEntry
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
    /// Paged result for audit log queries
    /// </summary>
    public class PagedAuditLogResultDto
    {
        public List<AuditLogEntry> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion
}
