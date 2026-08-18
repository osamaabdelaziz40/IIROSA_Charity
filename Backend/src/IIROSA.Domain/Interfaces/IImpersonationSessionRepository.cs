//using IIROSA.Domain.Contracts.Persistence;
//using IIROSA.Domain.Entities;

//namespace IIROSA.Domain.Interfaces;

///// <summary>
///// Repository interface for ImpersonationSession entity
///// </summary>
//public interface IImpersonationSessionRepository : IRepository<IIROSA.Domain.Entities.ImpersonationSession>
//{
//    /// <summary>
//    /// Get active impersonation session for a specific impersonator
//    /// </summary>
//    Task<IIROSA.Domain.Entities.ImpersonationSession?> GetActiveSessionByImpersonatorAsync(Guid impersonatorId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get all active impersonation sessions
//    /// </summary>
//    Task<IEnumerable<IIROSA.Domain.Entities.ImpersonationSession>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get impersonation history for a specific user
//    /// </summary>
//    Task<IEnumerable<IIROSA.Domain.Entities.ImpersonationSession>> GetHistoryByUserAsync(Guid userId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get impersonation history with filters
//    /// </summary>
//    Task<IEnumerable<IIROSA.Domain.Entities.ImpersonationSession>> GetHistoryAsync(
//        Guid? impersonatorId = null,
//        Guid? impersonatedId = null,
//        DateTime? startDate = null,
//        DateTime? endDate = null,
//        CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Increment action counter for an active session
//    /// </summary>
//    Task IncrementActionsCountAsync(Guid sessionId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Terminate an active session
//    /// </summary>
//    Task TerminateSessionAsync(Guid sessionId, Guid? terminatedBy = null, string? reason = null, CancellationToken cancellationToken = default);
//}
