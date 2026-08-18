using Framework.Core.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Repositories;

/// <summary>
/// Repository interface for ImpersonationSession entity
/// </summary>
public interface IImpersonationSessionRepository : IRepositoryBase<Framework.Identity.Data.AppIdentityDbContext, Framework.Identity.Data.Entities.ImpersonationSession>
{
    /// <summary>
    /// Get active impersonation session for a specific impersonator
    /// </summary>
    Task<Framework.Identity.Data.Entities.ImpersonationSession?> GetActiveSessionByImpersonatorAsync(Guid impersonatorId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active impersonation sessions
    /// </summary>
    Task<IEnumerable<Framework.Identity.Data.Entities.ImpersonationSession>> GetActiveSessionsAsync(System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get impersonation history for a specific user
    /// </summary>
    Task<IEnumerable<Framework.Identity.Data.Entities.ImpersonationSession>> GetHistoryByUserAsync(Guid userId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get impersonation history with filters
    /// </summary>
    Task<IEnumerable<Framework.Identity.Data.Entities.ImpersonationSession>> GetHistoryAsync(
        Guid? impersonatorId = null,
        Guid? impersonatedId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment action counter for an active session
    /// </summary>
    Task IncrementActionsCountAsync(Guid sessionId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate an active session
    /// </summary>
    Task TerminateSessionAsync(Guid sessionId, Guid? terminatedBy = null, string? reason = null, System.Threading.CancellationToken cancellationToken = default);
}
