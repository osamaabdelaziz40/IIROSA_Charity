using System;
using System.Threading.Tasks;
using Framework.Identity.Data.Entities;

namespace Framework.Identity.Data.Services.Interfaces;

/// <summary>
/// Interface for refresh token management service
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Create a new refresh token for a user
    /// </summary>
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? ipAddress = null, string? userAgent = null);

    /// <summary>
    /// Get a refresh token by its value
    /// </summary>
    Task<RefreshToken?> GetRefreshTokenAsync(string token);

    /// <summary>
    /// Validate a refresh token and return if it's active
    /// </summary>
    Task<(bool isValid, RefreshToken? token)> ValidateRefreshTokenAsync(string token);

    /// <summary>
    /// Mark a refresh token as used
    /// </summary>
    Task MarkAsUsedAsync(RefreshToken refreshToken, string? replacedByToken = null);

    /// <summary>
    /// Revoke a refresh token
    /// </summary>
    Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? reason = null);

    /// <summary>
    /// Revoke all refresh tokens for a user (e.g., on logout/password change)
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, string? reason = null);

    /// <summary>
    /// Clean up expired and revoked tokens
    /// </summary>
    Task CleanupExpiredTokensAsync();
}
