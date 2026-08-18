using IIROSA.Application.DTOs.AuditLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IIROSA.Application.Interfaces
{
    /// <summary>
    /// Service for managing audit logging operations
    /// </summary>
    public interface IAuditService
    {
        #region Automatic Logging Operations

        /// <summary>
        /// Log entity creation operation
        /// </summary>
        Task LogCreationAsync<T>(T entity, string userName, string ipAddress = null, string userAgent = null, Guid? correlationId = null) where T : class;

        /// <summary>
        /// Log entity update operation with field changes
        /// </summary>
        Task LogUpdateAsync<T>(T entity, Dictionary<string, (object old, object newVal)> fieldChanges, string userName, string ipAddress = null, string userAgent = null, Guid? correlationId = null) where T : class;

        /// <summary>
        /// Log entity deletion operation
        /// </summary>
        Task LogDeletionAsync<T>(T entity, string userName, string ipAddress = null, string userAgent = null, Guid? correlationId = null) where T : class;

        /// <summary>
        /// Log user login event
        /// </summary>
        Task LogLoginAsync(Guid userId, string userName, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// Log user logout event
        /// </summary>
        Task LogLogoutAsync(Guid userId, string userName, string ipAddress = null, string userAgent = null);

        /// <summary>
        /// Log failed login attempt
        /// </summary>
        Task LogFailedLoginAsync(string attemptedUsername, string ipAddress = null, string userAgent = null, string reason = null);

        #endregion

        #region Query Operations

        /// <summary>
        /// Get filtered audit logs
        /// </summary>
        Task<PagedAuditLogResponseDto<AuditLogListDto>> GetFilteredAsync(AuditLogFilterDto filter);

        /// <summary>
        /// Get audit log details by ID
        /// </summary>
        Task<AuditLogDetailDto> GetByIdAsync(Guid auditLogId);

        /// <summary>
        /// Search audit logs by keyword
        /// </summary>
        Task<PagedAuditLogResponseDto<AuditLogListDto>> SearchAsync(AuditLogSearchDto search);

        /// <summary>
        /// Get entity change history
        /// </summary>
        Task<EntityHistoryDto> GetEntityHistoryAsync(Guid entityId, string entityType);

        /// <summary>
        /// Get user activity log with summary
        /// </summary>
        Task<UserActivityLogDto> GetUserActivityAsync(UserActivityFilterDto filter);

        #endregion

        #region Comparison Operations

        /// <summary>
        /// Compare two versions of a record
        /// </summary>
        Task<RecordVersionComparisonDto> CompareVersionsAsync(Guid beforeAuditLogId, Guid afterAuditLogId);

        /// <summary>
        /// Preview restore operation
        /// </summary>
        Task<RestorePreviewDto> PreviewRestoreAsync(Guid auditLogId);

        /// <summary>
        /// Restore entity to previous version
        /// </summary>
        Task<RestoreResultDto> RestoreVersionAsync(Guid auditLogId, string userName, string ipAddress, string userAgent, string reason = null);

        #endregion

        #region Export Operations

        /// <summary>
        /// Export audit logs
        /// </summary>
        Task<AuditLogExportResultDto> ExportAsync(AuditLogExportRequestDto request, string userName, string ipAddress, string userAgent);

        #endregion
    }
}
