using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using System;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services.Interfaces;

/// <summary>
/// Service interface for User Impersonation functionality
/// Implements use cases UC-19.1 through UC-19.8
/// </summary>
public interface IImpersonationService
{
    /// <summary>
    /// Start an impersonation session (UC-19.1, UC-19.5)
    /// </summary>
    Task<StartImpersonationResponse> StartImpersonationAsync(Guid impersonatorId, string targetUsername, string ipAddress, string? userAgent = null, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// End an impersonation session and return to original account (UC-19.2)
    /// </summary>
    Task<EndImpersonationResponse> EndImpersonationAsync(Guid sessionId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active impersonation sessions (UC-19.3)
    /// </summary>
    Task<System.Collections.Generic.List<ActiveImpersonationSessionDto>> GetActiveSessionsAsync(System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate an active session (admin override) (UC-19.4)
    /// </summary>
    Task TerminateSessionAsync(Guid sessionId, Guid terminatedBy, string? reason = null, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Search for users by username/email for quick impersonation (UC-19.5)
    /// </summary>
    Task<System.Collections.Generic.List<UserSearchResult>> SearchUsersAsync(string searchTerm, Guid requesterId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get impersonation history with filters (UC-19.6)
    /// </summary>
    Task<System.Collections.Generic.List<ImpersonationSessionHistoryDto>> GetHistoryAsync(ImpersonationHistoryFilter filter, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current impersonation status for a user
    /// </summary>
    Task<ImpersonationStatusDto> GetStatusAsync(Guid userId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Get impersonation settings (UC-19.8)
    /// </summary>
    Task<ImpersonationSettingsDto> GetSettingsAsync(System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Update impersonation settings (UC-19.8)
    /// </summary>
    Task UpdateSettingsAsync(ImpersonationSettingsDto settings, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if a user can impersonate another user
    /// </summary>
    Task<(bool CanImpersonate, string? Reason)> ValidateImpersonationAsync(Guid impersonatorId, Guid targetUserId, System.Threading.CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment action counter for an active session (UC-19.7)
    /// </summary>
    Task IncrementActionCountAsync(Guid sessionId, System.Threading.CancellationToken cancellationToken = default);
}
