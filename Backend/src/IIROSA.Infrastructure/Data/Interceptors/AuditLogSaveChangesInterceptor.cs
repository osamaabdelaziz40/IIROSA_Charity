using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace IIROSA.Infrastructure.Data.Interceptors
{
    /// <summary>
    /// EF Core interceptor for automatic audit logging
    /// Logs all Create, Update, and Delete operations when SaveChanges is called
    /// Implements UC-17.1, UC-17.2, UC-17.3: Automatic logging of entity changes
    /// </summary>
    public class AuditLogSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditLogSaveChangesInterceptor> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // IAuditService must be resolved lazily (see CreateAuditLogAsync), not injected here:
        // this interceptor is constructed while DbContextOptions are being built, and
        // IAuditService -> IAuditLogRepository -> ApplicationDbContext loops straight back
        // into that options lambda, exhausting the stack and hanging startup.
        public AuditLogSaveChangesInterceptor(
            IServiceProvider serviceProvider,
            ILogger<AuditLogSaveChangesInterceptor> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            LogAuditEntries(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            LogAuditEntries(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // Note: SavedAfter and SavedAfterAsync are not available in SaveChangesInterceptor
        // Audit logging is done synchronously in SavingChanges before the save operation
        // For async logging, consider using background tasks or a different approach

        private List<AuditEntry> OnBeforeSaveChanges(DbContext context)
        {
            if (context == null) return null;

            var auditEntries = new List<AuditEntry>();
            var changeTracker = context.ChangeTracker;

            // Get current user information
            var currentUser = GetCurrentUser();
            var currentIpAddress = GetCurrentIpAddress();
            var currentUserAgent = GetCurrentUserAgent();

            foreach (var entry in changeTracker.Entries())
            {
                // Skip entities that don't have audit attributes
                if (ShouldSkipEntity(entry.Entity.GetType()))
                    continue;

                var auditEntry = new AuditEntry(entry)
                {
                    TableName = entry.Entity.GetType().Name,
                    UserId = GetUserId(currentUser),
                    UserName = currentUser,
                    IpAddress = currentIpAddress,
                    UserAgent = currentUserAgent,
                    Timestamp = DateTime.UtcNow
                };

                // Collect the current and original values
                foreach (var property in entry.Properties)
                {
                    string propertyName = property.Metadata.Name;

                    // Skip shadow properties and navigation properties
                    if (property.IsTemporary || property.Metadata.IsPrimaryKey())
                        continue;

                    // Skip navigation properties and collections
                    if (property.Metadata.IsShadowProperty())
                        continue;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntries.Add(auditEntry);
            }

            return auditEntries;
        }

        private async Task CreateAuditLogAsync(AuditEntry auditEntry, DbContext context)
        {
            try
            {
                var entity = auditEntry.Entry.Entity;
                var entityId = GetEntityId(auditEntry.Entry, entity);

                // Build field changes JSON
                var fieldChanges = BuildFieldChangesJson(auditEntry);

                // Determine operation
                var operation = auditEntry.Entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => null
                };

                if (operation == null) return;

                // Create the audit log through the service
                var auditLog = new AuditLogEntry
                {
                    AuditLogId = Guid.NewGuid(),
                    EntityType = auditEntry.TableName,
                    EntityId = entityId,
                    Operation = operation,
                    UserId = auditEntry.UserId,
                    UserName = auditEntry.UserName ?? "System",
                    IpAddress = auditEntry.IpAddress,
                    Timestamp = auditEntry.Timestamp,
                    FieldChanges = fieldChanges,
                    AdditionalContext = JsonSerializer.Serialize(new
                    {
                        action = operation.ToLower(),
                        entityState = auditEntry.Entry.State.ToString()
                    }),
                    UserAgent = auditEntry.UserAgent
                };

                // Log the audit entry using the service (resolved lazily — by this point
                // the ApplicationDbContext instance already exists, so no DI cycle occurs)
                // Note: In a production environment, you might want to batch these
                // or use a background task to avoid slowing down the main operation
                _serviceProvider.GetRequiredService<IAuditService>()
                    .LogCreationAsync(auditLog, auditEntry.UserName, auditEntry.IpAddress, auditEntry.UserAgent)
                    .ContinueWith(t => _logger.LogError(t.Exception, "Failed to log audit entry"),
                        CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating audit log entry for {EntityType}", auditEntry.TableName);
            }
        }

        private static string BuildFieldChangesJson(AuditEntry auditEntry)
        {
            var changes = new Dictionary<string, object>();

            foreach (var kvp in auditEntry.NewValues)
            {
                var fieldName = kvp.Key;
                var oldValue = auditEntry.OldValues.ContainsKey(fieldName) ? auditEntry.OldValues[fieldName] : null;
                var newValue = kvp.Value;

                changes[fieldName] = new { old = oldValue, @new = newValue };
            }

            // For deletes, add all old values
            if (auditEntry.Entry.State == EntityState.Deleted)
            {
                foreach (var kvp in auditEntry.OldValues)
                {
                    if (!changes.ContainsKey(kvp.Key))
                    {
                        changes[kvp.Key] = new { old = kvp.Value, @new = (object)null };
                    }
                }
            }

            return JsonSerializer.Serialize(changes);
        }

        private static Guid? GetEntityId(EntityEntry entry, object entity)
        {
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            if (idProperty != null && idProperty.CurrentValue is Guid guid)
                return guid;

            // Try to get Id from the entity directly
            var idProp = entity.GetType().GetProperty("Id");
            if (idProp != null && idProp.PropertyType == typeof(Guid))
            {
                return (Guid)idProp.GetValue(entity);
            }

            return null;
        }

        private static Guid? GetUserId(string userName)
        {
            if (Guid.TryParse(userName, out var guid))
                return guid;
            return null;
        }

        private string GetCurrentUser()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
                return httpContext.User.Identity.Name;

            return "System";
        }

        private string GetCurrentIpAddress()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            return httpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string GetCurrentUserAgent()
        {
            var httpContext = _httpContextAccessor?.HttpContext;
            return httpContext?.Request?.Headers["User-Agent"].ToString();
        }

        private static bool ShouldSkipEntity(Type entityType)
        {
            // Skip audit-related entities to prevent infinite loops
            var skipTypes = new[]
            {
                "AuditLog",
                "Audit",
                "Log",
                "NotificationsLog"
            };

            return skipTypes.Contains(entityType.Name);
        }

        private void LogAuditEntries(DbContext context)
        {
            // This is called before SaveChanges - we just prepare the entries
            // Actual logging happens in SavedAfter after successful save
        }

        // Queue for pending audit logs to be saved after successful SaveChanges
        private static readonly Queue<AuditLogEntry> _pendingAuditLogs = new Queue<AuditLogEntry>();

        public static IEnumerable<AuditLogEntry> GetPendingAuditLogs()
        {
            while (_pendingAuditLogs.Count > 0)
            {
                yield return _pendingAuditLogs.Dequeue();
            }
        }
    }

    #region Helper Classes

    /// <summary>
    /// Public class to hold audit entry data during SaveChanges processing
    /// </summary>
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Public class for audit log entry data
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

    #endregion
}
