using IIROSA.Application.DTOs.AuditLog;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace IIROSA.Api.Controllers
{
    /// <summary>
    /// Controller for audit log management
    /// Provides endpoints for viewing, searching, and exporting audit logs
    /// All endpoints require Admin or Super Admin roles
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogsController(
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor)
        {
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
        }

        private string CurrentUserName => User?.Identity?.Name ?? "Unknown";
        private string CurrentIpAddress => _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        private string CurrentUserAgent => _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString();

        #region UC-17.7: View Audit Logs

        /// <summary>
        /// Get filtered audit logs with pagination
        /// UC-17.7: View Audit Logs
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Paged list of audit logs</returns>
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(PagedAuditLogResponseDto<AuditLogListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedAuditLogResponseDto<AuditLogListDto>>> GetFilteredAuditLogs([FromQuery] AuditLogFilterDto filter)
        {
            var result = await _auditService.GetFilteredAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get audit log details by ID
        /// </summary>
        /// <param name="id">Audit log ID</param>
        /// <returns>Audit log details</returns>
        [HttpGet("{id}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(AuditLogDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AuditLogDetailDto>> GetById(Guid id)
        {
            var result = await _auditService.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        #endregion

        #region UC-17.8: View Entity Change History

        /// <summary>
        /// Get complete change history for a specific entity
        /// UC-17.8: View Entity Change History
        /// </summary>
        /// <param name="entityId">Entity ID</param>
        /// <param name="entityType">Entity type (e.g., "Family", "Orphan")</param>
        /// <returns>Entity change history</returns>
        [HttpGet("history/{entityId}/{entityType}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(EntityHistoryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<EntityHistoryDto>> GetEntityHistory(Guid entityId, string entityType)
        {
            var result = await _auditService.GetEntityHistoryAsync(entityId, entityType);
            return Ok(result);
        }

        #endregion

        #region UC-17.9: View User Activity Log

        /// <summary>
        /// Get activity log for a specific user with summary statistics
        /// UC-17.9: View User Activity Log
        /// </summary>
        /// <param name="filter">User activity filter criteria</param>
        /// <returns>User activity log with summary</returns>
        [HttpGet("activity/user")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(UserActivityLogDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserActivityLogDto>> GetUserActivity([FromQuery] UserActivityFilterDto filter)
        {
            var result = await _auditService.GetUserActivityAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Get activity summary for a specific user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="dateFrom">Optional start date</param>
        /// <param name="dateTo">Optional end date</param>
        /// <returns>User activity summary</returns>
        [HttpGet("activity/summary/{userId}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(UserActivitySummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<UserActivitySummaryDto>> GetUserActivitySummary(
            Guid userId,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var filter = new UserActivityFilterDto
            {
                UserId = userId,
                DateFrom = dateFrom,
                DateTo = dateTo
            };

            var result = await _auditService.GetUserActivityAsync(filter);
            return Ok(result.Summary);
        }

        #endregion

        #region UC-17.10: Export Audit Logs

        /// <summary>
        /// Export audit logs to Excel, CSV, or JSON
        /// UC-17.10: Export Audit Logs
        /// </summary>
        /// <param name="request">Export request parameters</param>
        /// <returns>Exported file</returns>
        [HttpPost("export")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportAuditLogs([FromBody] AuditLogExportRequestDto request)
        {
            var result = await _auditService.ExportAsync(
                request,
                CurrentUserName,
                CurrentIpAddress,
                CurrentUserAgent);

            return File(
                result.FileContents,
                result.ContentType,
                result.FileName);
        }

        #endregion

        #region UC-17.11: Search Audit Logs

        /// <summary>
        /// Search audit logs by keyword across field values, usernames, and entity types
        /// UC-17.11: Search Audit Logs
        /// </summary>
        /// <param name="search">Search criteria</param>
        /// <returns>Matching audit logs</returns>
        [HttpPost("search")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(PagedAuditLogResponseDto<AuditLogListDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedAuditLogResponseDto<AuditLogListDto>>> SearchAuditLogs([FromBody] AuditLogSearchDto search)
        {
            var result = await _auditService.SearchAsync(search);
            return Ok(result);
        }

        #endregion

        #region UC-17.12: Compare Record Versions

        /// <summary>
        /// Compare two versions of a record to see differences
        /// UC-17.12: Compare Record Versions
        /// </summary>
        /// <param name="request">Comparison request with before and after audit log IDs</param>
        /// <returns>Side-by-side comparison</returns>
        [HttpPost("compare")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(RecordVersionComparisonDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RecordVersionComparisonDto>> CompareVersions([FromBody] CompareVersionsRequestDto request)
        {
            var result = await _auditService.CompareVersionsAsync(request.BeforeAuditLogId, request.AfterAuditLogId);
            return Ok(result);
        }

        /// <summary>
        /// Preview what a restore operation would change
        /// </summary>
        /// <param name="auditLogId">Audit log ID to preview restore from</param>
        /// <returns>Restore preview</returns>
        [HttpGet("restore/preview/{auditLogId}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(RestorePreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RestorePreviewDto>> PreviewRestore(Guid auditLogId)
        {
            var result = await _auditService.PreviewRestoreAsync(auditLogId);
            return Ok(result);
        }

        #endregion

        #region UC-17.13: Restore Record Version

        /// <summary>
        /// Restore an entity to a previous version from audit log
        /// UC-17.13: Restore Record Version
        /// </summary>
        /// <param name="request">Restore request with audit log ID and optional reason</param>
        /// <returns>Restore result</returns>
        [HttpPost("restore")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(RestoreResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RestoreResultDto>> RestoreVersion([FromBody] RestoreVersionRequestDto request)
        {
            var result = await _auditService.RestoreVersionAsync(
                request.AuditLogId,
                CurrentUserName,
                CurrentIpAddress,
                CurrentUserAgent,
                request.Reason);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        #endregion

        #region Helper DTOs for Controller

        /// <summary>
        /// Request DTO for comparing two versions
        /// </summary>
        public class CompareVersionsRequestDto
        {
            public Guid BeforeAuditLogId { get; set; }
            public Guid AfterAuditLogId { get; set; }
        }

        #endregion
    }
}
