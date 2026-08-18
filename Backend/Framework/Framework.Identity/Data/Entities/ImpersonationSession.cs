using Framework.Core.Data;
using Framework.Identity.Data.Entities;
using System;

namespace Framework.Identity.Data.Entities;

/// <summary>
/// Impersonation Session entity - Tracks admin impersonation of other users
/// Implements use cases UC-19.1 through UC-19.8
/// All audit fields are inherited from AuditEntityBase
/// </summary>
public class ImpersonationSession : FullAuditedEntityBase<Guid>
{
    /// <summary>
    /// The user who is initiating the impersonation (admin/super admin)
    /// </summary>
    public Guid ImpersonatorUserId { get; set; }

    /// <summary>
    /// Username of the impersonator (denormalized for audit)
    /// </summary>
    public string? ImpersonatorUserName { get; set; }

    /// <summary>
    /// The user whose identity is being assumed
    /// </summary>
    public Guid ImpersonatedUserId { get; set; }

    /// <summary>
    /// Username of the impersonated user (denormalized for audit)
    /// </summary>
    public string? ImpersonatedUserName { get; set; }

    /// <summary>
    /// Session start timestamp (UTC)
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Session end timestamp (UTC) - null for active sessions
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Indicates if session is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// IP address from which impersonation was initiated
    /// </summary>
    public string? OriginalUserIpAddress { get; set; }

    /// <summary>
    /// User-Agent string from impersonator's browser
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Number of actions performed during this session
    /// </summary>
    public int ActionsPerformedCount { get; set; } = 0;

    /// <summary>
    /// If session was terminated by another admin, their user ID
    /// </summary>
    public Guid? TerminatedBy { get; set; }

    /// <summary>
    /// Username of admin who terminated the session (denormalized)
    /// </summary>
    public string? TerminatedByName { get; set; }

    /// <summary>
    /// Reason for termination (if applicable)
    /// </summary>
    public string? TerminationReason { get; set; }

    /// <summary>
    /// Calculated session duration in seconds (computed property)
    /// </summary>
    public int? DurationSeconds =>
        EndTime.HasValue
            ? (int)(EndTime.Value - StartTime).TotalSeconds
            : (int)(DateTime.UtcNow - StartTime).TotalSeconds;

    // Navigation Properties
    /// <summary>
    /// Navigation to the impersonator user
    /// </summary>
    public virtual ApplicationUser? ImpersonatorUser { get; set; }

    /// <summary>
    /// Navigation to the impersonated user
    /// </summary>
    public virtual ApplicationUser? ImpersonatedUser { get; set; }
}
