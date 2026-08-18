//using IIROSA.Application.DTOs.UserManagement;

//namespace IIROSA.Application.Services;

///// <summary>
///// Service interface for User Impersonation functionality
///// Implements use cases UC-19.1 through UC-19.8
///// </summary>
//public interface IImpersonationService
//{
//    /// <summary>
//    /// Start an impersonation session (UC-19.1, UC-19.5)
//    /// </summary>
//    Task<StartImpersonationResponse> StartImpersonationAsync(Guid impersonatorId, string targetUsername, string ipAddress, string? userAgent = null, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// End an impersonation session and return to original account (UC-19.2)
//    /// </summary>
//    Task<EndImpersonationResponse> EndImpersonationAsync(Guid sessionId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get all active impersonation sessions (UC-19.3)
//    /// </summary>
//    Task<List<ActiveImpersonationSessionDto>> GetActiveSessionsAsync(CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Terminate an active session (admin override) (UC-19.4)
//    /// </summary>
//    Task TerminateSessionAsync(Guid sessionId, Guid terminatedBy, string? reason = null, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Search for users by username/email for quick impersonation (UC-19.5)
//    /// </summary>
//    Task<List<UserSearchResult>> SearchUsersAsync(string searchTerm, Guid requesterId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get impersonation history with filters (UC-19.6)
//    /// </summary>
//    Task<List<ImpersonationSessionHistoryDto>> GetHistoryAsync(ImpersonationHistoryFilter filter, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get current impersonation status for a user
//    /// </summary>
//    Task<ImpersonationStatusDto> GetStatusAsync(Guid userId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Get impersonation settings (UC-19.8)
//    /// </summary>
//    Task<ImpersonationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Update impersonation settings (UC-19.8)
//    /// </summary>
//    Task UpdateSettingsAsync(ImpersonationSettingsDto settings, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Validate if a user can impersonate another user
//    /// </summary>
//    Task<(bool CanImpersonate, string? Reason)> ValidateImpersonationAsync(Guid impersonatorId, Guid targetUserId, CancellationToken cancellationToken = default);

//    /// <summary>
//    /// Increment action counter for an active session (UC-19.7)
//    /// </summary>
//    Task IncrementActionCountAsync(Guid sessionId, CancellationToken cancellationToken = default);
//}
