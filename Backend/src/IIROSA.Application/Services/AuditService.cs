using IIROSA.Application.DTOs.AuditLog;
using IIROSA.Application.Enums;
using IIROSA.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Framework.Core.SharedServices.Entities;

namespace IIROSA.Application.Services
{
    /// <summary>
    /// Service for managing audit logging operations
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            IAuditLogRepository auditLogRepository,
            ILogger<AuditService> logger)
        {
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        #region Automatic Logging Operations

        public async Task LogCreationAsync<T>(T entity, string userName, string ipAddress = null, string userAgent = null, Guid? correlationId = null) where T : class
        {
            try
            {
                var entityId = GetEntityId(entity);
                var entityType = entity.GetType().Name;
                var fieldChanges = CaptureAllFieldValues(entity);

                var auditEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = AuditOperation.Create.ToString(),
                    UserId = GetUserId(userName),
                    UserName = userName ?? "System",
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = JsonSerializer.Serialize(fieldChanges),
                    AdditionalContext = JsonSerializer.Serialize(new { action = "create" }),
                    CorrelationId = correlationId,
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(auditEntry);
                _logger.LogInformation("Created audit log for {EntityType} {EntityId}", entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for {EntityType}", entity.GetType().Name);
                // Don't throw - audit logging failures shouldn't break the main operation
            }
        }

        public async Task LogUpdateAsync<T>(T entity, Dictionary<string, (object old, object newVal)> fieldChanges, string userName, string ipAddress = null, string userAgent = null, Guid? correlationId = null) where T : class
        {
            try
            {
                var entityId = GetEntityId(entity);
                var entityType = entity.GetType().Name;

                var changes = fieldChanges.ToDictionary(
                    f => f.Key,
                    f => new { old = FormatValue(f.Value.old), @new = FormatValue(f.Value.newVal) }
                );

                var auditEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = AuditOperation.Update.ToString(),
                    UserId = GetUserId(userName),
                    UserName = userName ?? "System",
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = JsonSerializer.Serialize(changes),
                    AdditionalContext = JsonSerializer.Serialize(new
                    {
                        action = "update",
                        fieldsChanged = fieldChanges.Count
                    }),
                    CorrelationId = correlationId,
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(auditEntry);
                _logger.LogInformation("Created audit log for update on {EntityType} {EntityId}", entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for update on {EntityType}", entity.GetType().Name);
            }
        }

        public async Task LogDeletionAsync<T>(T entity, string userName, string ipAddress = null, string userAgent = null, Guid? correlationId = null) where T : class
        {
            try
            {
                var entityId = GetEntityId(entity);
                var entityType = entity.GetType().Name;
                var fieldChanges = CaptureAllFieldValues(entity);

                var auditEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Operation = AuditOperation.Delete.ToString(),
                    UserId = GetUserId(userName),
                    UserName = userName ?? "System",
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = JsonSerializer.Serialize(fieldChanges),
                    AdditionalContext = JsonSerializer.Serialize(new { action = "delete" }),
                    CorrelationId = correlationId,
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(auditEntry);
                _logger.LogInformation("Created audit log for deletion of {EntityType} {EntityId}", entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create audit log for deletion of {EntityType}", entity.GetType().Name);
            }
        }

        public async Task LogLoginAsync(Guid userId, string userName, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var auditEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = "UserLogin",
                    EntityId = userId,
                    Operation = AuditOperation.Login.ToString(),
                    UserId = userId,
                    UserName = userName,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = JsonSerializer.Serialize(new { loginTime = DateTime.UtcNow }),
                    AdditionalContext = JsonSerializer.Serialize(new { action = "login" }),
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(auditEntry);
                _logger.LogInformation("Logged login for user {UserName}", userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log login for user {UserName}", userName);
            }
        }

        public async Task LogLogoutAsync(Guid userId, string userName, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var auditEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = "UserLogout",
                    EntityId = userId,
                    Operation = AuditOperation.Logout.ToString(),
                    UserId = userId,
                    UserName = userName,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = JsonSerializer.Serialize(new { logoutTime = DateTime.UtcNow }),
                    AdditionalContext = JsonSerializer.Serialize(new { action = "logout" }),
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(auditEntry);
                _logger.LogInformation("Logged logout for user {UserName}", userName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log logout for user {UserName}", userName);
            }
        }

        public async Task LogFailedLoginAsync(string attemptedUsername, string ipAddress = null, string userAgent = null, string reason = null)
        {
            try
            {
                var auditEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = "FailedLogin",
                    EntityId = null,
                    Operation = AuditOperation.FailedLogin.ToString(),
                    UserId = null,
                    UserName = attemptedUsername,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = JsonSerializer.Serialize(new
                    {
                        attemptedUsername,
                        timestamp = DateTime.UtcNow,
                        reason = reason ?? "Invalid credentials"
                    }),
                    AdditionalContext = JsonSerializer.Serialize(new { action = "failed_login", reason }),
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(auditEntry);
                _logger.LogWarning("Logged failed login attempt for username {UserName} from IP {IpAddress}", attemptedUsername, ipAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log failed login for username {UserName}", attemptedUsername);
            }
        }

        #endregion

        #region Query Operations

        public async Task<PagedAuditLogResponseDto<AuditLogListDto>> GetFilteredAsync(AuditLogFilterDto filter)
        {
            var result = await _auditLogRepository.GetFilteredAsync(filter);

            return new PagedAuditLogResponseDto<AuditLogListDto>
            {
                Items = result.Items.Select(MapToListDto).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };
        }

        public async Task<AuditLogDetailDto> GetByIdAsync(Guid auditLogId)
        {
            var entry = await _auditLogRepository.GetByIdAsync(auditLogId);
            if (entry == null) return null;

            return new AuditLogDetailDto
            {
                AuditLogId = entry.AuditLogId,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Operation = entry.Operation,
                UserId = entry.UserId,
                UserName = entry.UserName,
                IpAddress = entry.IpAddress,
                Timestamp = entry.Timestamp,
                FieldChanges = entry.FieldChanges,
                AdditionalContext = entry.AdditionalContext,
                CorrelationId = entry.CorrelationId,
                UserAgent = entry.UserAgent,
                ParsedFieldChanges = ParseFieldChanges(entry.FieldChanges),
                ParsedContext = ParseJson(entry.AdditionalContext)
            };
        }

        public async Task<PagedAuditLogResponseDto<AuditLogListDto>> SearchAsync(AuditLogSearchDto search)
        {
            var result = await _auditLogRepository.SearchAsync(search);

            return new PagedAuditLogResponseDto<AuditLogListDto>
            {
                Items = result.Items.Select(MapToListDto).ToList(),
                TotalCount = result.TotalCount,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize
            };
        }

        public async Task<EntityHistoryDto> GetEntityHistoryAsync(Guid entityId, string entityType)
        {
            return await _auditLogRepository.GetEntityHistoryAsync(entityId, entityType);
        }

        public async Task<UserActivityLogDto> GetUserActivityAsync(UserActivityFilterDto filter)
        {
            var summary = await _auditLogRepository.GetUserActivitySummaryAsync(
                filter.UserId,
                filter.DateFrom,
                filter.DateTo);

            var activities = await _auditLogRepository.GetUserActivityAsync(filter);

            return new UserActivityLogDto
            {
                UserId = filter.UserId,
                UserName = activities.Items.FirstOrDefault()?.UserName ?? "Unknown",
                Activities = activities.Items.Select(MapToDetailDto).ToList(),
                Summary = summary
            };
        }

        #endregion

        #region Comparison Operations

        public async Task<RecordVersionComparisonDto> CompareVersionsAsync(Guid beforeAuditLogId, Guid afterAuditLogId)
        {
            var (before, after) = await _auditLogRepository.GetForComparisonAsync(beforeAuditLogId, afterAuditLogId);

            if (before == null || after == null)
                throw new InvalidOperationException("One or both audit log entries not found");

            var beforeChanges = ParseFieldChanges(before.FieldChanges);
            var afterChanges = ParseFieldChanges(after.FieldChanges);

            var allFields = beforeChanges
                .Concat(afterChanges)
                .Select(f => f.FieldName)
                .Distinct()
                .ToList();

            var comparisons = allFields.Select(field =>
            {
                var beforeVal = beforeChanges.FirstOrDefault(f => f.FieldName == field);
                var afterVal = afterChanges.FirstOrDefault(f => f.FieldName == field);

                return new FieldComparisonDto
                {
                    FieldName = field,
                    BeforeValue = beforeVal?.NewValue ?? beforeVal?.OldValue,
                    AfterValue = afterVal?.NewValue ?? afterVal?.OldValue,
                    HasChanged = beforeVal?.NewValue != afterVal?.NewValue,
                    ChangeType = DetermineChangeType(beforeVal, afterVal)
                };
            }).ToList();

            return new RecordVersionComparisonDto
            {
                BeforeAuditLogId = beforeAuditLogId,
                AfterAuditLogId = afterAuditLogId,
                BeforeTimestamp = before.Timestamp,
                AfterTimestamp = after.Timestamp,
                FieldComparisons = comparisons
            };
        }

        public async Task<RestorePreviewDto> PreviewRestoreAsync(Guid auditLogId)
        {
            var entry = await _auditLogRepository.GetForRestoreAsync(auditLogId);
            if (entry == null)
                throw new InvalidOperationException("Audit log entry not found");

            var fieldChanges = ParseFieldChanges(entry.FieldChanges);

            return new RestorePreviewDto
            {
                AuditLogId = auditLogId,
                VersionTimestamp = entry.Timestamp,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                ChangesToApply = fieldChanges,
                Warning = entry.EntityId == null ?
                    "This operation will create a new entity with these values." :
                    "This will restore the entity to the state shown below."
            };
        }

        public async Task<RestoreResultDto> RestoreVersionAsync(Guid auditLogId, string userName, string ipAddress, string userAgent, string reason = null)
        {
            try
            {
                var entry = await _auditLogRepository.GetForRestoreAsync(auditLogId);
                if (entry == null)
                    return new RestoreResultDto { Success = false, Message = "Audit log entry not found" };

                // This is a placeholder - the actual restore logic would depend on the entity type
                // and would need to be implemented per entity or through a generic repository
                var restoreEntry = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = entry.EntityType,
                    EntityId = entry.EntityId,
                    Operation = AuditOperation.Restore.ToString(),
                    UserId = GetUserId(userName),
                    UserName = userName,
                    IpAddress = ipAddress,
                    Timestamp = DateTime.UtcNow,
                    FieldChanges = entry.FieldChanges,
                    AdditionalContext = JsonSerializer.Serialize(new
                    {
                        action = "restore",
                        restoredFromAuditLogId = auditLogId,
                        reason
                    }),
                    UserAgent = userAgent
                };

                await _auditLogRepository.AddAsync(restoreEntry);

                return new RestoreResultDto
                {
                    Success = true,
                    Message = "Restore operation logged successfully",
                    NewAuditLogId = restoreEntry.AuditLogId,
                    RestoredEntityId = entry.EntityId,
                    Warnings = new List<string>
                    {
                        "Note: Actual entity restoration requires implementation per entity type"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore version from audit log {AuditLogId}", auditLogId);
                return new RestoreResultDto
                {
                    Success = false,
                    Message = $"Failed to restore: {ex.Message}"
                };
            }
        }

        #endregion

        #region Export Operations

        public async Task<AuditLogExportResultDto> ExportAsync(AuditLogExportRequestDto request, string userName, string ipAddress, string userAgent)
        {
            try
            {
                var entries = await _auditLogRepository.GetForExportAsync(request);

                byte[] fileContents;
                string fileName;
                string contentType;

                switch (request.Format)
                {
                    case ExportFormat.Excel:
                        // Excel format - use CSV (Excel compatible)
                        (fileContents, fileName) = GenerateCsvExport(entries, request);
                        contentType = "text/csv";
                        break;

                    case ExportFormat.Csv:
                        (fileContents, fileName) = GenerateCsvExport(entries, request);
                        contentType = "text/csv";
                        break;

                    case ExportFormat.Json:
                        (fileContents, fileName) = GenerateJsonExport(entries, request);
                        contentType = "application/json";
                        break;

                    default:
                        throw new InvalidOperationException($"Unsupported export format: {request.Format}");
                }

                // Log the export action
                await LogExportAsync(request, entries.Count, userName, ipAddress, userAgent);

                return new AuditLogExportResultDto
                {
                    FileContents = fileContents,
                    FileName = fileName,
                    ContentType = contentType,
                    RecordCount = entries.Count,
                    ExportGeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export audit logs");
                throw;
            }
        }

        #endregion

        #region Private Helper Methods

        private Guid? GetUserId(string userName)
        {
            // This would need to resolve the user ID from the username
            // For now, return null - this would be implemented with IUserService
            if (Guid.TryParse(userName, out var guid))
                return guid;
            return null;
        }

        private Guid? GetEntityId<T>(T entity) where T : class
        {
            var idProperty = entity.GetType().GetProperty("Id");
            if (idProperty != null && idProperty.PropertyType == typeof(Guid))
            {
                return (Guid)idProperty.GetValue(entity);
            }
            return null;
        }

        private Dictionary<string, object> CaptureAllFieldValues<T>(T entity) where T : class
        {
            var result = new Dictionary<string, object>();
            var properties = entity.GetType().GetProperties();

            foreach (var prop in properties)
            {
                if (prop.Name == "Id") continue;
                if (prop.Name.EndsWith("Navigation")) continue;
                if (prop.GetIndexParameters().Length > 0) continue;

                try
                {
                    var value = prop.GetValue(entity);
                    result[prop.Name] = FormatValue(value);
                }
                catch
                {
                    // Skip properties that can't be read
                }
            }

            return result;
        }

        private static string FormatValue(object value)
        {
            return value switch
            {
                null => null,
                DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
                bool b => b.ToString(),
                string s => s,
                _ => value.ToString()
            };
        }

        private static AuditLogListDto MapToListDto(AuditLogEntry entry)
        {
            var summary = GetOperationSummary(entry.Operation, entry.FieldChanges);

            return new AuditLogListDto
            {
                AuditLogId = entry.AuditLogId,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Operation = entry.Operation,
                OperationDisplayName = entry.Operation switch
                {
                    "Create" => "Created",
                    "Update" => "Updated",
                    "Delete" => "Deleted",
                    "Login" => "Logged In",
                    "Logout" => "Logged Out",
                    "FailedLogin" => "Failed Login",
                    "Export" => "Exported",
                    "Restore" => "Restored",
                    _ => entry.Operation
                },
                UserName = entry.UserName,
                IpAddress = entry.IpAddress,
                Timestamp = entry.Timestamp,
                Summary = summary
            };
        }

        private static AuditLogDetailDto MapToDetailDto(AuditLogEntry entry)
        {
            return new AuditLogDetailDto
            {
                AuditLogId = entry.AuditLogId,
                EntityType = entry.EntityType,
                EntityId = entry.EntityId,
                Operation = entry.Operation,
                UserId = entry.UserId,
                UserName = entry.UserName,
                IpAddress = entry.IpAddress,
                Timestamp = entry.Timestamp,
                FieldChanges = entry.FieldChanges,
                AdditionalContext = entry.AdditionalContext,
                CorrelationId = entry.CorrelationId,
                UserAgent = entry.UserAgent,
                ParsedFieldChanges = ParseFieldChanges(entry.FieldChanges),
                ParsedContext = ParseJson(entry.AdditionalContext)
            };
        }

        private static string GetOperationSummary(string operation, string fieldChanges)
        {
            if (string.IsNullOrEmpty(fieldChanges))
                return operation;

            try
            {
                var changes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(fieldChanges);
                var count = changes?.Count ?? 0;
                return $"{operation} ({count} field{(count == 1 ? "" : "s")})";
            }
            catch
            {
                return operation;
            }
        }

        private static List<FieldChangeDto> ParseFieldChanges(string fieldChangesJson)
        {
            if (string.IsNullOrEmpty(fieldChangesJson))
                return new List<FieldChangeDto>();

            try
            {
                var changes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(fieldChangesJson);
                var result = new List<FieldChangeDto>();

                if (changes != null)
                {
                    foreach (var kvp in changes)
                    {
                        var oldValue = kvp.Value.TryGetProperty("old", out var oldVal) ?
                            oldVal.ValueKind != JsonValueKind.Null ? oldVal.ToString() : null : null;
                        var newValue = kvp.Value.TryGetProperty("new", out var newVal) ?
                            newVal.ValueKind != JsonValueKind.Null ? newVal.ToString() : null : null;

                        result.Add(new FieldChangeDto
                        {
                            FieldName = kvp.Key,
                            OldValue = oldValue,
                            NewValue = newValue
                        });
                    }
                }

                return result;
            }
            catch
            {
                return new List<FieldChangeDto>();
            }
        }

        private static Dictionary<string, object> ParseJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new Dictionary<string, object>();

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private static ChangeType DetermineChangeType(FieldChangeDto before, FieldChangeDto after)
        {
            if (before == null && after != null)
                return ChangeType.Added;
            if (before != null && after == null)
                return ChangeType.Removed;
            if (before?.NewValue != after?.NewValue)
                return ChangeType.Modified;
            return ChangeType.Unchanged;
        }

        private async Task LogExportAsync(AuditLogExportRequestDto request, int recordCount, string userName, string ipAddress, string userAgent)
        {
            var exportEntry = new AuditLogEntry
            {
                AuditLogId = Guid.NewGuid(),
                EntityType = "AuditLogExport",
                EntityId = null,
                Operation = AuditOperation.Export.ToString(),
                UserId = GetUserId(userName),
                UserName = userName,
                IpAddress = ipAddress,
                Timestamp = DateTime.UtcNow,
                FieldChanges = JsonSerializer.Serialize(new
                {
                    dateRange = new { request.DateFrom, request.DateTo },
                    filters = new { request.EntityType, request.Operation, request.UserId },
                    format = request.Format.ToString(),
                    recordCount
                }),
                AdditionalContext = JsonSerializer.Serialize(new { action = "export" }),
                UserAgent = userAgent
            };

            await _auditLogRepository.AddAsync(exportEntry);
        }

        private static (byte[], string) GenerateCsvExport(List<AuditLogEntry> entries, AuditLogExportRequestDto request)
        {
            var csv = new StringBuilder();

            // Headers
            csv.AppendLine("Timestamp,Operation,EntityType,EntityId,UserName,IpAddress");

            // Data
            foreach (var entry in entries)
            {
                csv.AppendLine($"\"{entry.Timestamp:yyyy-MM-dd HH:mm:ss}\",\"{entry.Operation}\",\"{entry.EntityType}\",\"{entry.EntityId}\",\"{entry.UserName}\",\"{entry.IpAddress}\"");
            }

            var fileName = $"AuditLogs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            return (Encoding.UTF8.GetBytes(csv.ToString()), fileName);
        }

        private static (byte[], string) GenerateJsonExport(List<AuditLogEntry> entries, AuditLogExportRequestDto request)
        {
            var json = JsonSerializer.Serialize(entries, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            var fileName = $"AuditLogs_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
            return (Encoding.UTF8.GetBytes(json), fileName);
        }

        #endregion
    }
}
