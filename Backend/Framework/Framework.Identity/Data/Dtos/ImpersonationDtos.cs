using System;
using System.Collections.Generic;

namespace Framework.Identity.Data.Dtos;

/// <summary>
/// Request DTO to start impersonation
/// </summary>
public class StartImpersonationRequest
{
    /// <summary>
    /// Username or email of the user to impersonate
    /// </summary>
    public string TargetUsername { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for starting impersonation
/// </summary>
public class StartImpersonationResponse
{
    /// <summary>
    /// New authentication token for impersonated user
    /// </summary>
    public string ImpersonationToken { get; set; } = string.Empty;

    /// <summary>
    /// Session ID for tracking
    /// </summary>
    public Guid SessionId { get; set; }

    /// <summary>
    /// User being impersonated
    /// </summary>
    public ImpersonatedUserInfo ImpersonatedUser { get; set; } = new();

    /// <summary>
    /// Session expiry time
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Response DTO for ending impersonation
/// </summary>
public class EndImpersonationResponse
{
    /// <summary>
    /// Original user token (restored)
    /// </summary>
    public string OriginalToken { get; set; } = string.Empty;

    /// <summary>
    /// Session duration in seconds
    /// </summary>
    public int SessionDurationSeconds { get; set; }

    /// <summary>
    /// Number of actions performed during session
    /// </summary>
    public int ActionsPerformed { get; set; }
}

/// <summary>
/// DTO for impersonated user info
/// </summary>
public class ImpersonatedUserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public IList<string> Roles { get; set; } = new List<string>();
}

/// <summary>
/// DTO for active impersonation session
/// </summary>
public class ActiveImpersonationSessionDto
{
    public Guid SessionId { get; set; }
    public Guid ImpersonatorUserId { get; set; }
    public string? ImpersonatorUserName { get; set; } = string.Empty;
    public string? ImpersonatorEmail { get; set; } = string.Empty;
    public string? ImpersonatorRole { get; set; } = string.Empty;
    public Guid ImpersonatedUserId { get; set; }
    public string? ImpersonatedUserName { get; set; } = string.Empty;
    public string? ImpersonatedEmail { get; set; } = string.Empty;
    public string? ImpersonatedRole { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string? OriginalUserIpAddress { get; set; }
    public int ActionsPerformedCount { get; set; }
    public int DurationMinutes => (int)(DateTime.UtcNow - StartTime).TotalMinutes;
}

/// <summary>
/// DTO for impersonation session history
/// </summary>
public class ImpersonationSessionHistoryDto
{
    public Guid SessionId { get; set; }
    public Guid ImpersonatorUserId { get; set; }
    public string? ImpersonatorUserName { get; set; } = string.Empty;
    public Guid ImpersonatedUserId { get; set; }
    public string? ImpersonatedUserName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsActive { get; set; }
    public int ActionsPerformedCount { get; set; }
    public string? TerminationReason { get; set; }
    public string? TerminatedByName { get; set; }
}

/// <summary>
/// DTO for filtering impersonation history
/// </summary>
public class ImpersonationHistoryFilter
{
    public Guid? ImpersonatorId { get; set; }
    public Guid? ImpersonatedId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO for user search in impersonation
/// </summary>
public class UserSearchResult
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Role { get; set; }
    public bool IsActive { get; set; }
    public bool CanImpersonate { get; set; }
}

/// <summary>
/// Request DTO for terminating another admin's session
/// </summary>
public class TerminateSessionRequest
{
    /// <summary>
    /// Reason for termination
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for impersonation settings
/// </summary>
public class ImpersonationSettingsDto
{
    public int MaxSessionDurationHours { get; set; } = 8;
    public int WarningThresholdMinutes { get; set; } = 15;
    public bool AllowSessionExtension { get; set; } = true;
    public int MaxExtensionsPerSession { get; set; } = 2;
    public bool RequireConfirmation { get; set; } = true;
    public bool LogAllActions { get; set; } = true;
    public bool NotifyImpersonatedUser { get; set; } = false;
    public bool IpRestrictionEnabled { get; set; } = false;
    public int LogRetentionDays { get; set; } = 365;
}

/// <summary>
/// DTO for current impersonation status
/// </summary>
public class ImpersonationStatusDto
{
    public bool IsImpersonating { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? OriginalUserId { get; set; }
    public string? OriginalUserName { get; set; }
    public DateTime SessionStartTime { get; set; }
    public DateTime SessionExpiresAt { get; set; }
    public ImpersonatedUserInfo? CurrentUser { get; set; }
}
