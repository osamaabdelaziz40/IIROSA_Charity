using System;
using Framework.Core.Data;

namespace Framework.Identity.Data.Entities;

/// <summary>
/// Refresh token entity for JWT token renewal
/// </summary>
public class RefreshToken : FullAuditedEntityBase<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevocationReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Navigation property
    public virtual ApplicationUser? User { get; set; }

    /// <summary>
    /// Check if the refresh token is currently active
    /// </summary>
    public bool IsActive => !IsUsed && !IsRevoked && ExpiresAt > DateTime.UtcNow;

    /// <summary>
    /// Check if the refresh token is expired
    /// </summary>
    public bool IsExpired => ExpiresAt < DateTime.UtcNow;
}
