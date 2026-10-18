using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
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
using Framework.Core.SharedServices;
using Framework.Core.SharedServices.Entities;

namespace IIROSA.Infrastructure.Data.Interceptors
{
    /// <summary>
    /// EF Core interceptor for automatic audit logging.
    /// Captures every Create (Added) and Update (Modified) when SaveChanges is called on
    /// ApplicationDbContext and writes one AuditLog row per changed entity to CommonsDbContext
    /// after the business save succeeds. Implements UC-17.1 / UC-17.2: automatic logging of
    /// entity changes, with charity attribution for tenancy-scoped audit views.
    /// </summary>
    public class AuditLogSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AuditLogSaveChangesInterceptor> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Pending entries captured in SavingChanges and persisted in SavedChanges, only after
        // the business save succeeded. The interceptor is registered scoped and resolved inside
        // the scoped AddDbContext options lambda, so this instance state lives per request scope.
        // It is cleared at the start of every capture and on save failure.
        private List<CapturedEntry> _pendingEntries;

        // IAuditService must be resolved lazily (see PersistAsync), not injected here:
        // this interceptor is constructed while DbContextOptions are being built, and
        // IAuditService -> IAuditLogRepository -> ApplicationDbContext loops straight back
        // into that options lambda, exhausting the stack and hanging startup. The same
        // rule is applied to ICurrentUserService and CommonsDbContext: both are resolved
        // per save, never in the constructor.
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
            CaptureEntries(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CaptureEntries(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            Persist(eventData.Context);
            return base.SavedChanges(eventData, result);
        }

        public override ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            var persistTask = PersistAsync(eventData.Context, cancellationToken);
            if (persistTask.IsCompletedSuccessfully)
                return new ValueTask<int>(result);

            return AwaitPersistThenReturnResultAsync(persistTask, result);
        }

        private async ValueTask<int> AwaitPersistThenReturnResultAsync(Task persistTask, int result)
        {
            // PersistAsync never throws (it swallows and logs), so the business result is unaffected.
            await persistTask.ConfigureAwait(false);
            return result;
        }

        public override void SaveChangesFailed(DbContextErrorEventData eventData)
        {
            _pendingEntries = null; // a failed business save must not be audited
            base.SaveChangesFailed(eventData);
        }

        public override Task SaveChangesFailedAsync(
            DbContextErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            _pendingEntries = null;
            return base.SaveChangesFailedAsync(eventData, cancellationToken);
        }

        /// <summary>
        /// Snapshot the change tracker before the save: original values are still intact and
        /// the HTTP context (user, IP, user agent) is still available. Values that EF generates
        /// during SaveChanges (sequential Guid keys) are read later, in the persist phase.
        /// </summary>
        private void CaptureEntries(DbContext context)
        {
            _pendingEntries = null;
            if (context == null) return;

            ICurrentUserService currentUser = null;
            try
            {
                currentUser = _serviceProvider.GetService<ICurrentUserService>();
            }
            catch (Exception ex)
            {
                // Design-time / background saves may run without the full application container.
                _logger.LogWarning(ex, "Could not resolve ICurrentUserService for audit capture");
            }

            var userName = Truncate(
                currentUser is { IsAuthenticated: true }
                    ? _httpContextAccessor?.HttpContext?.User?.Identity?.Name
                    : null,
                256) ?? "System";

            var entries = new List<CapturedEntry>();
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.State is not (EntityState.Added or EntityState.Modified)) continue;
                if (ShouldSkipEntity(entry.Metadata.ClrType)) continue;

                var captured = new CapturedEntry
                {
                    Entry = entry,
                    EntityType = Truncate(entry.Metadata.ClrType.Name, 256),
                    Operation = entry.State == EntityState.Added ? "Create" : "Update",
                    UserId = currentUser?.UserId,
                    UserName = userName,
                    IpAddress = Truncate(GetCurrentIpAddress(), 64),
                    UserAgent = Truncate(GetCurrentUserAgent(), 512),
                    Timestamp = DateTime.UtcNow
                };

                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;

                    // Skip keys, shadow properties and the maintenance columns BaseDbContext
                    // stamps on every save (CreatedBy/CreatedOn/UpdatedBy/UpdatedOn) — they are
                    // bookkeeping, not business changes. IsDeleted/DeletedOn/DeletedBy are kept:
                    // a soft delete is business-relevant.
                    if (property.Metadata.IsPrimaryKey()) continue;
                    if (property.Metadata.IsShadowProperty()) continue;
                    if (MaintenanceColumns.Contains(propertyName)) continue;
                    if (SensitiveFields.Contains(propertyName)) continue;

                    if (entry.State == EntityState.Added)
                    {
                        captured.NewValues[propertyName] = SerializeSafe(property.CurrentValue);
                    }
                    else if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                    {
                        captured.OldValues[propertyName] = SerializeSafe(property.OriginalValue);
                        captured.NewValues[propertyName] = SerializeSafe(property.CurrentValue);
                    }
                }

                // An update that changed nothing (e.g. only the stamped audit columns) is not logged.
                if (entry.State == EntityState.Modified && captured.NewValues.Count == 0) continue;

                entries.Add(captured);
            }

            if (entries.Count > 0)
                _pendingEntries = entries;
        }

        /// <summary>
        /// Write the captured entries as AuditLog rows. Runs only after the business save
        /// succeeded. Everything is best-effort: an audit failure must never fail the
        /// business operation, so exceptions are swallowed and logged.
        /// </summary>
        private async Task PersistAsync(DbContext context, CancellationToken cancellationToken)
        {
            var entries = TakePendingEntries();
            if (entries == null || entries.Count == 0) return;

            try
            {
                // CommonsDbContext has no interceptor registered and its constructor chain never
                // touches ApplicationDbContext, so this save cannot recurse or re-enter.
                var commons = _serviceProvider.GetRequiredService<CommonsDbContext>();

                var logs = new List<AuditLog>(entries.Count);
                foreach (var captured in entries)
                {
                    logs.Add(new AuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        EntityType = captured.EntityType,
                        EntityId = GetKeyGuid(captured.Entry),
                        Operation = captured.Operation,
                        UserId = captured.UserId,
                        UserName = captured.UserName ?? "System",
                        IpAddress = captured.IpAddress,
                        Timestamp = captured.Timestamp,
                        FieldChanges = BuildFieldChangesJson(captured),
                        AdditionalContext = JsonSerializer.Serialize(new
                        {
                            action = captured.Operation.ToLowerInvariant(),
                            entityState = captured.Operation == "Create" ? "Added" : "Modified"
                        }),
                        UserAgent = captured.UserAgent,
                        CharityId = ResolveCharityId(captured.Entry)
                    });
                }

                commons.AddRange(logs);
                await commons.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Audit logging failed after a successful business save; the business operation is unaffected");
            }
        }

        private void Persist(DbContext context)
        {
            var entries = TakePendingEntries();
            if (entries == null || entries.Count == 0) return;

            try
            {
                var commons = _serviceProvider.GetRequiredService<CommonsDbContext>();

                var logs = new List<AuditLog>(entries.Count);
                foreach (var captured in entries)
                {
                    logs.Add(new AuditLog
                    {
                        AuditLogId = Guid.NewGuid(),
                        EntityType = captured.EntityType,
                        EntityId = GetKeyGuid(captured.Entry),
                        Operation = captured.Operation,
                        UserId = captured.UserId,
                        UserName = captured.UserName ?? "System",
                        IpAddress = captured.IpAddress,
                        Timestamp = captured.Timestamp,
                        FieldChanges = BuildFieldChangesJson(captured),
                        AdditionalContext = JsonSerializer.Serialize(new
                        {
                            action = captured.Operation.ToLowerInvariant(),
                            entityState = captured.Operation == "Create" ? "Added" : "Modified"
                        }),
                        UserAgent = captured.UserAgent,
                        CharityId = ResolveCharityId(captured.Entry)
                    });
                }

                commons.AddRange(logs);
                commons.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Audit logging failed after a successful business save; the business operation is unaffected");
            }
        }

        private List<CapturedEntry> TakePendingEntries()
        {
            var entries = _pendingEntries;
            _pendingEntries = null;
            return entries;
        }

        private static string BuildFieldChangesJson(CapturedEntry captured)
        {
            var changes = new Dictionary<string, object>();

            foreach (var kvp in captured.NewValues)
            {
                var oldValue = captured.OldValues.TryGetValue(kvp.Key, out var old) ? old : null;
                changes[kvp.Key] = new { old = oldValue, @new = kvp.Value };
            }

            return JsonSerializer.Serialize(changes);
        }

        /// <summary>Primary key value for the (now saved) entity; null for non-Guid keys.</summary>
        private static Guid? GetKeyGuid(EntityEntry entry)
        {
            var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            return idProperty?.CurrentValue as Guid?;
        }

        /// <summary>
        /// Charity attribution: the Charity entity is the tenant root (its key is the charity id),
        /// entities carrying a CharityId property are attributed to that charity, everything else
        /// falls back to the acting user's charity (null for head-office / system actions).
        /// </summary>
        private Guid? ResolveCharityId(EntityEntry entry)
        {
            if (entry.Entity is Charity charity)
                return charity.Id;

            var charityIdProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CharityId");
            if (charityIdProperty?.CurrentValue is Guid charityId)
                return charityId;

            try
            {
                return _serviceProvider.GetService<ICurrentUserService>()?.CharityId;
            }
            catch
            {
                return null;
            }
        }

        private static readonly HashSet<string> MaintenanceColumns = new HashSet<string>
        {
            "CreatedBy", "CreatedOn", "UpdatedBy", "UpdatedOn"
        };

        // No entity carries these today; the guard exists so a future field cannot leak.
        private static readonly HashSet<string> SensitiveFields = new HashSet<string>
        {
            "Password", "Secret", "Token"
        };

        /// <summary>Mask values JSON cannot represent usefully and humanise enums.</summary>
        private static object SerializeSafe(object value)
        {
            return value switch
            {
                null => null,
                byte[] _ => "[binary]",
                Enum enumValue => enumValue.ToString(),
                _ => value
            };
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
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

        /// <summary>
        /// Snapshot of one changed entity, taken before the save. The EntityEntry reference is
        /// kept deliberately: values EF generates during SaveChanges (sequential Guid keys) are
        /// only readable after the save completes.
        /// </summary>
        private sealed class CapturedEntry
        {
            public EntityEntry Entry { get; init; }
            public string EntityType { get; init; }
            public string Operation { get; init; }
            public Guid? UserId { get; init; }
            public string UserName { get; init; }
            public string IpAddress { get; init; }
            public string UserAgent { get; init; }
            public DateTime Timestamp { get; init; }
            public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
            public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        }
    }
}
