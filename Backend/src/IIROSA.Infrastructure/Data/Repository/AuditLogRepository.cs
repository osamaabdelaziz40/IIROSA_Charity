using Dapper;
using IIROSA.Application.DTOs.AuditLog;
using IIROSA.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Framework.Core.SharedServices.Entities;
using Framework.Core.SharedServices;
using IIROSA.Infrastructure.Data;
using IIROSA.Application.Enums;

namespace IIROSA.Infrastructure.Data.Repository
{
    /// <summary>
    /// Repository implementation for AuditLog using CommonsDbContext
    /// </summary>
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly CommonsDbContext _context;
        private readonly ILogger<AuditLogRepository> _logger;
        private readonly ApplicationDbContext _applicationContext;

        public AuditLogRepository(
            CommonsDbContext context,
            ApplicationDbContext applicationContext,
            ILogger<AuditLogRepository> logger)
        {
            _context = context;
            _applicationContext = applicationContext;
            _logger = logger;
        }

        #region Create Operations

        public async Task AddAsync(AuditLogEntry entry)
        {
            var auditLog = MapToEntity(entry);
            await _context.AuditLogs.AddAsync(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<AuditLogEntry> entries)
        {
            var auditLogs = entries.Select(MapToEntity).ToList();
            await _context.AuditLogs.AddRangeAsync(auditLogs);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Query Operations

        public async Task<AuditLogEntry> GetByIdAsync(Guid auditLogId)
        {
            var auditLog = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuditLogId == auditLogId);

            return auditLog == null ? null : MapToEntry(auditLog);
        }

        public async Task<List<AuditLogEntry>> GetByEntityIdAsync(Guid entityId, string entityType)
        {
            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityId == entityId && a.EntityType == entityType)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return auditLogs.Select(MapToEntry).ToList();
        }

        public async Task<List<AuditLogEntry>> GetByUserIdAsync(Guid userId)
        {
            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return auditLogs.Select(MapToEntry).ToList();
        }

        public async Task<PagedAuditLogResultDto> GetFilteredAsync(AuditLogFilterDto filter)
        {
            var query = _context.AuditLogs.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);

            if (!string.IsNullOrEmpty(filter.Operation))
                query = query.Where(a => a.Operation == filter.Operation);

            if (filter.UserId.HasValue)
                query = query.Where(a => a.UserId == filter.UserId.Value);

            if (filter.EntityId.HasValue)
                query = query.Where(a => a.EntityId == filter.EntityId.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(a => a.Timestamp >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(a => a.Timestamp <= filter.DateTo.Value);

            if (!string.IsNullOrEmpty(filter.IpAddress))
                query = query.Where(a => a.IpAddress.Contains(filter.IpAddress));

            if (filter.CorrelationId.HasValue)
                query = query.Where(a => a.CorrelationId == filter.CorrelationId.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = ApplySorting(query, filter.SortBy, filter.SortDirection);

            // Apply pagination
            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedAuditLogResultDto
            {
                Items = items.Select(MapToEntry).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        public async Task<PagedAuditLogResultDto> SearchAsync(AuditLogSearchDto search)
        {
            var keyword = $"%{search.Keyword}%";

            var query = _context.AuditLogs.AsNoTracking()
                .Where(a => EF.Functions.Like(a.UserName, keyword) ||
                           EF.Functions.Like(a.EntityType, keyword) ||
                           EF.Functions.Like(a.FieldChanges, keyword) ||
                           EF.Functions.Like(a.Operation, keyword));

            // Apply optional filters
            if (!string.IsNullOrEmpty(search.EntityType))
                query = query.Where(a => a.EntityType == search.EntityType);

            if (!string.IsNullOrEmpty(search.Operation))
                query = query.Where(a => a.Operation == search.Operation);

            if (search.DateFrom.HasValue)
                query = query.Where(a => a.Timestamp >= search.DateFrom.Value);

            if (search.DateTo.HasValue)
                query = query.Where(a => a.Timestamp <= search.DateTo.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((search.PageNumber - 1) * search.PageSize)
                .Take(search.PageSize)
                .ToListAsync();

            return new PagedAuditLogResultDto
            {
                Items = items.Select(MapToEntry).ToList(),
                TotalCount = totalCount,
                PageNumber = search.PageNumber,
                PageSize = search.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)search.PageSize)
            };
        }

        public async Task<EntityHistoryDto> GetEntityHistoryAsync(Guid entityId, string entityType)
        {
            var auditLogs = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.EntityId == entityId && a.EntityType == entityType)
                .OrderBy(a => a.Timestamp)
                .ToListAsync();

            if (!auditLogs.Any())
            {
                return new EntityHistoryDto
                {
                    EntityId = entityId,
                    EntityType = entityType,
                    HistoryEntries = new List<AuditLogDetailDto>(),
                    TotalChanges = 0,
                    FirstChange = DateTime.UtcNow,
                    LastChange = DateTime.UtcNow
                };
            }

            return new EntityHistoryDto
            {
                EntityId = entityId,
                EntityType = entityType,
                TotalChanges = auditLogs.Count,
                FirstChange = auditLogs.First().Timestamp,
                LastChange = auditLogs.Last().Timestamp,
                HistoryEntries = auditLogs.Select(a => new AuditLogDetailDto
                {
                    AuditLogId = a.AuditLogId,
                    EntityType = a.EntityType,
                    EntityId = a.EntityId,
                    Operation = a.Operation,
                    UserId = a.UserId,
                    UserName = a.UserName,
                    IpAddress = a.IpAddress,
                    Timestamp = a.Timestamp,
                    FieldChanges = a.FieldChanges,
                    AdditionalContext = a.AdditionalContext,
                    CorrelationId = a.CorrelationId,
                    UserAgent = a.UserAgent,
                    ParsedFieldChanges = ParseFieldChanges(a.FieldChanges),
                    ParsedContext = ParseJson(a.AdditionalContext)
                }).ToList()
            };
        }

        public async Task<UserActivitySummaryDto> GetUserActivitySummaryAsync(Guid userId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.AuditLogs.AsNoTracking()
                .Where(a => a.UserId == userId);

            if (dateFrom.HasValue)
                query = query.Where(a => a.Timestamp >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(a => a.Timestamp <= dateTo.Value);

            var auditLogs = await query.ToListAsync();

            if (!auditLogs.Any())
            {
                return new UserActivitySummaryDto
                {
                    TotalActions = 0,
                    ActionsByType = new Dictionary<string, int>(),
                    ActionsByEntity = new Dictionary<string, int>(),
                    FirstActivityDate = DateTime.UtcNow,
                    LastActivityDate = DateTime.UtcNow,
                    UniqueEntitiesAffected = 0,
                    UniqueIpAddresses = 0
                };
            }

            return new UserActivitySummaryDto
            {
                TotalActions = auditLogs.Count,
                ActionsByType = auditLogs.GroupBy(a => a.Operation)
                    .ToDictionary(g => g.Key, g => g.Count()),
                ActionsByEntity = auditLogs.Where(a => a.EntityId.HasValue)
                    .GroupBy(a => a.EntityType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                FirstActivityDate = auditLogs.Min(a => a.Timestamp),
                LastActivityDate = auditLogs.Max(a => a.Timestamp),
                UniqueEntitiesAffected = auditLogs.Where(a => a.EntityId.HasValue)
                    .Select(a => a.EntityId.Value)
                    .Distinct()
                    .Count(),
                UniqueIpAddresses = auditLogs.Where(a => !string.IsNullOrEmpty(a.IpAddress))
                    .Select(a => a.IpAddress)
                    .Distinct()
                    .Count()
            };
        }

        public async Task<PagedAuditLogResultDto> GetUserActivityAsync(UserActivityFilterDto filter)
        {
            var query = _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId == filter.UserId);

            if (filter.DateFrom.HasValue)
                query = query.Where(a => a.Timestamp >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(a => a.Timestamp <= filter.DateTo.Value);

            if (!string.IsNullOrEmpty(filter.Operation))
                query = query.Where(a => a.Operation == filter.Operation);

            if (!string.IsNullOrEmpty(filter.EntityType))
                query = query.Where(a => a.EntityType == filter.EntityType);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedAuditLogResultDto
            {
                Items = items.Select(MapToEntry).ToList(),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
            };
        }

        #endregion

        #region Export Operations

        public async Task<List<AuditLogEntry>> GetForExportAsync(AuditLogExportRequestDto request)
        {
            var query = _context.AuditLogs.AsNoTracking()
                .Where(a => a.Timestamp >= request.DateFrom && a.Timestamp <= request.DateTo);

            if (!string.IsNullOrEmpty(request.EntityType))
                query = query.Where(a => a.EntityType == request.EntityType);

            if (!string.IsNullOrEmpty(request.Operation))
                query = query.Where(a => a.Operation == request.Operation);

            if (request.UserId.HasValue)
                query = query.Where(a => a.UserId == request.UserId.Value);

            // If not including field changes, select only core fields
            if (!request.IncludeFieldChanges)
            {
                var coreFields = await query
                    .Select(a => new AuditLogEntry
                    {
                        AuditLogId = a.AuditLogId,
                        EntityType = a.EntityType,
                        EntityId = a.EntityId,
                        Operation = a.Operation,
                        UserId = a.UserId,
                        UserName = a.UserName,
                        IpAddress = a.IpAddress,
                        Timestamp = a.Timestamp,
                        FieldChanges = null,
                        AdditionalContext = request.IncludeContext ? a.AdditionalContext : null,
                        CorrelationId = a.CorrelationId,
                        UserAgent = a.UserAgent
                    })
                    .OrderByDescending(a => a.Timestamp)
                    .ToListAsync();

                return coreFields;
            }

            var fullEntries = await query
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return fullEntries.Select(MapToEntry).ToList();
        }

        #endregion

        #region Comparison Operations

        public async Task<(AuditLogEntry before, AuditLogEntry after)> GetForComparisonAsync(Guid beforeAuditLogId, Guid afterAuditLogId)
        {
            var beforeEntry = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuditLogId == beforeAuditLogId);

            var afterEntry = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuditLogId == afterAuditLogId);

            return (MapToEntry(beforeEntry), MapToEntry(afterEntry));
        }

        public async Task<AuditLogEntry> GetForRestoreAsync(Guid auditLogId)
        {
            var auditLog = await _context.AuditLogs
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AuditLogId == auditLogId);

            return auditLog == null ? null : MapToEntry(auditLog);
        }

        #endregion

        #region Private Helper Methods

        private static AuditLog MapToEntity(AuditLogEntry entry)
        {
            return new AuditLog
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
                UserAgent = entry.UserAgent
            };
        }

        private static AuditLogEntry MapToEntry(AuditLog entity)
        {
            if (entity == null) return null;

            return new AuditLogEntry
            {
                AuditLogId = entity.AuditLogId,
                EntityType = entity.EntityType,
                EntityId = entity.EntityId,
                Operation = entity.Operation,
                UserId = entity.UserId,
                UserName = entity.UserName,
                IpAddress = entity.IpAddress,
                Timestamp = entity.Timestamp,
                FieldChanges = entity.FieldChanges,
                AdditionalContext = entity.AdditionalContext,
                CorrelationId = entity.CorrelationId,
                UserAgent = entity.UserAgent
            };
        }

        private static IQueryable<AuditLog> ApplySorting(IQueryable<AuditLog> query, string sortBy, string sortDirection)
        {
            return (sortBy?.ToLower(), sortDirection?.ToLower()) switch
            {
                ("timestamp", "asc") => query.OrderBy(a => a.Timestamp),
                ("timestamp", "desc") => query.OrderByDescending(a => a.Timestamp),
                ("entitytype", "asc") => query.OrderBy(a => a.EntityType),
                ("entitytype", "desc") => query.OrderByDescending(a => a.EntityType),
                ("operation", "asc") => query.OrderBy(a => a.Operation),
                ("operation", "desc") => query.OrderByDescending(a => a.Operation),
                ("username", "asc") => query.OrderBy(a => a.UserName),
                ("username", "desc") => query.OrderByDescending(a => a.UserName),
                _ => query.OrderByDescending(a => a.Timestamp)
            };
        }

        private static List<FieldChangeDto> ParseFieldChanges(string fieldChangesJson)
        {
            if (string.IsNullOrEmpty(fieldChangesJson))
                return new List<FieldChangeDto>();

            try
            {
                var changes = System.Text.Json.JsonDocument.Parse(fieldChangesJson);
                var result = new List<FieldChangeDto>();

                foreach (var property in changes.RootElement.EnumerateObject())
                {
                    var oldValue = property.Value.TryGetProperty("old", out var oldVal) ?
                        oldVal.ValueKind != JsonValueKind.Null ? oldVal.ToString() : null : null;
                    var newValue = property.Value.TryGetProperty("new", out var newVal) ?
                        newVal.ValueKind != JsonValueKind.Null ? newVal.ToString() : null : null;

                    result.Add(new FieldChangeDto
                    {
                        FieldName = property.Name,
                        OldValue = oldValue,
                        NewValue = newValue
                    });
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
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var result = new Dictionary<string, object>();

                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    result[property.Name] = property.Value.ToString();
                }

                return result;
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        #endregion
    }
}
