using System;
using System.Linq;
using System.Threading.Tasks;
using Framework.Identity.Data;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Framework.Identity.Data.Services;

/// <summary>
/// Refresh token management service implementation
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private readonly AppIdentityDbContext _context;
    private readonly ILogger<RefreshTokenService> _logger;

    public RefreshTokenService(
        AppIdentityDbContext context,
        ILogger<RefreshTokenService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new refresh token for a user
    /// </summary>
    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? ipAddress = null, string? userAgent = null)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days default
            CreatedOn = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        _context.Set<RefreshToken>().Add(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created refresh token for user {UserId}", userId);

        return refreshToken;
    }

    /// <summary>
    /// Get a refresh token by its value
    /// </summary>
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    /// <summary>
    /// Validate a refresh token and return if it's active
    /// </summary>
    public async Task<(bool isValid, RefreshToken? token)> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await GetRefreshTokenAsync(token);

        if (refreshToken == null)
        {
            _logger.LogWarning("Refresh token not found: {Token}", token[..10] + "...");
            return (false, null);
        }

        if (!refreshToken.IsActive)
        {
            _logger.LogWarning("Refresh token is not active for user {UserId}. IsUsed: {IsUsed}, IsRevoked: {IsRevoked}, IsExpired: {IsExpired}",
                refreshToken.UserId, refreshToken.IsUsed, refreshToken.IsRevoked, refreshToken.IsExpired);
            return (false, refreshToken);
        }

        return (true, refreshToken);
    }

    /// <summary>
    /// Mark a refresh token as used
    /// </summary>
    public async Task MarkAsUsedAsync(RefreshToken refreshToken, string? replacedByToken = null)
    {
        refreshToken.IsUsed = true;
        refreshToken.ReplacedByToken = replacedByToken;
        refreshToken.UpdatedOn = DateTime.UtcNow;

        _context.Set<RefreshToken>().Update(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Marked refresh token as used for user {UserId}", refreshToken.UserId);
    }

    /// <summary>
    /// Revoke a refresh token
    /// </summary>
    public async Task RevokeRefreshTokenAsync(RefreshToken refreshToken, string? reason = null)
    {
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevocationReason = reason;
        refreshToken.UpdatedOn = DateTime.UtcNow;

        _context.Set<RefreshToken>().Update(refreshToken);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Revoked refresh token for user {UserId}. Reason: {Reason}",
            refreshToken.UserId, reason ?? "Not specified");
    }

    /// <summary>
    /// Revoke all refresh tokens for a user
    /// </summary>
    public async Task RevokeAllUserTokensAsync(Guid userId, string? reason = null)
    {
        var activeTokens = await _context.Set<RefreshToken>()
            .Where(rt => rt.UserId == userId && !rt.IsUsed && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevocationReason = reason;
            token.UpdatedOn = DateTime.UtcNow;
        }

        _context.Set<RefreshToken>().UpdateRange(activeTokens);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Revoked {Count} refresh tokens for user {UserId}. Reason: {Reason}",
            activeTokens.Count, userId, reason ?? "Not specified");
    }

    /// <summary>
    /// Clean up expired and revoked tokens older than 30 days
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var tokensToDelete = await _context.Set<RefreshToken>()
            .Where(rt => (rt.ExpiresAt < DateTime.UtcNow || rt.IsRevoked) && rt.UpdatedOn < cutoffDate)
            .ToListAsync();

        if (tokensToDelete.Any())
        {
            _context.Set<RefreshToken>().RemoveRange(tokensToDelete);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Cleaned up {Count} expired/revoked refresh tokens", tokensToDelete.Count);
        }
    }

    /// <summary>
    /// Generate a cryptographically secure random token
    /// </summary>
    private string GenerateSecureToken()
    {
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[64];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "")
            .Replace(",", "");
    }
}
