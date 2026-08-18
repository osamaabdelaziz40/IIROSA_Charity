//using System.Security.Claims;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Microsoft.IdentityModel.JsonWebTokens;
//using IIROSA.Application.DTOs.UserManagement;
//using IIROSA.Domain.Entities;
//using IIROSA.Domain.Interfaces;
//using Framework.Identity.Data;
//using Framework.Identity.Data.Entities;

//namespace IIROSA.Application.Services;

///// <summary>
///// Service implementation for User Impersonation functionality
///// Implements use cases UC-19.1 through UC-19.8
///// </summary>
//public class ImpersonationService : IImpersonationService
//{
//    private readonly IImpersonationSessionRepository _sessionRepository;
//    private readonly IUnitOfWork _unitOfWork;
//    private readonly ILogger<ImpersonationService> _logger;
//    private readonly UserManager<ApplicationUser> _userManager;
//    private readonly ITokenService _tokenService;
//    private readonly ImpersonationSettings _settings;

//    public ImpersonationService(
//        IImpersonationSessionRepository sessionRepository,
//        IUnitOfWork unitOfWork,
//        ILogger<ImpersonationService> logger,
//        UserManager<ApplicationUser> userManager,
//        ITokenService tokenService,
//        IOptions<ImpersonationSettings> settings)
//    {
//        _sessionRepository = sessionRepository;
//        _unitOfWork = unitOfWork;
//        _logger = logger;
//        _userManager = userManager;
//        _tokenService = tokenService;
//        _settings = settings.Value;
//    }

//    /// <summary>
//    /// Start an impersonation session (UC-19.1, UC-19.5)
//    /// </summary>
//    public async Task<StartImpersonationResponse> StartImpersonationAsync(
//        Guid impersonatorId,
//        string targetUsername,
//        string ipAddress,
//        string? userAgent = null,
//        CancellationToken cancellationToken = default)
//    {
//        // Get impersonator user
//        var impersonator = await _userManager.FindByIdAsync(impersonatorId.ToString());
//        if (impersonator == null)
//        {
//            throw new InvalidOperationException("Impersonator user not found.");
//        }

//        // Get target user
//        var targetUser = await _userManager.FindByNameAsync(targetUsername)
//                        ?? await _userManager.FindByEmailAsync(targetUsername);
//        if (targetUser == null)
//        {
//            throw new InvalidOperationException($"User '{targetUsername}' not found.");
//        }

//        // Validate impersonation permission
//        var (canImpersonate, reason) = await ValidateImpersonationAsync(impersonatorId, targetUser.Id, cancellationToken);
//        if (!canImpersonate)
//        {
//            throw new UnauthorizedAccessException(reason ?? "Cannot impersonate this user.");
//        }

//        // Check for existing active session
//        var existingSession = await _sessionRepository.GetActiveSessionByImpersonatorAsync(impersonatorId, cancellationToken);
//        if (existingSession != null)
//        {
//            // End existing session first
//            await EndImpersonationAsync(existingSession.Id, cancellationToken);
//        }

//        // Get user roles
//        var impersonatorRoles = await _userManager.GetRolesAsync(impersonator);
//        var targetRoles = await _userManager.GetRolesAsync(targetUser);

//        // Create impersonation session
//        var session = new ImpersonationSession
//        {
//            Id = Guid.NewGuid(),
//            ImpersonatorUserId = impersonatorId,
//            ImpersonatorUserName = impersonator.UserName ?? impersonator.Email,
//            ImpersonatorEmail = impersonator.Email,
//            ImpersonatedUserId = targetUser.Id,
//            ImpersonatedUserName = targetUser.UserName ?? targetUser.Email,
//            ImpersonatedEmail = targetUser.Email,
//            StartTime = DateTime.UtcNow,
//            IsActive = true,
//            OriginalUserIpAddress = ipAddress,
//            UserAgent = userAgent,
//            ActionsPerformedCount = 0
//        };

//        await _sessionRepository.AddAsync(session);
//        await _unitOfWork.SaveChangesAsync(cancellationToken);

//        _logger.LogInformation(
//            "Impersonation started: {Impersonator} -> {TargetUser}",
//            impersonator.UserName,
//            targetUser.UserName);

//        // Generate token for target user with impersonation context
//        var targetToken = await GenerateImpersonationTokenAsync(targetUser, session.Id, impersonatorId);

//        return new StartImpersonationResponse
//        {
//            ImpersonationToken = targetToken,
//            SessionId = session.Id,
//            ImpersonatedUser = new ImpersonatedUserInfo
//            {
//                Id = targetUser.Id,
//                Username = targetUser.UserName ?? targetUser.Email,
//                Email = targetUser.Email ?? string.Empty,
//                FullName = targetUser.FullName,
//                Roles = targetRoles.ToList()
//            },
//            ExpiresAt = DateTime.UtcNow.AddHours(_settings.MaxSessionDurationHours)
//        };
//    }

//    /// <summary>
//    /// End an impersonation session (UC-19.2)
//    /// </summary>
//    public async Task<EndImpersonationResponse> EndImpersonationAsync(
//        Guid sessionId,
//        CancellationToken cancellationToken = default)
//    {
//        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
//        if (session == null)
//        {
//            throw new InvalidOperationException("Session not found.");
//        }

//        if (!session.IsActive)
//        {
//            throw new InvalidOperationException("Session is already ended.");
//        }

//        // Terminate session
//        await TerminateSessionInternalAsync(session, null, "User ended impersonation");

//        // Generate new token for original user
//        var originalUser = await _userManager.FindByIdAsync(session.ImpersonatorUserId.ToString());
//        if (originalUser == null)
//        {
//            throw new InvalidOperationException("Original user not found.");
//        }

//        var originalToken = await _tokenService.GenerateTokenAsync(originalUser);

//        _logger.LogInformation(
//            "Impersonation ended: {Impersonator} <- {TargetUser}, Duration: {Duration}s, Actions: {Actions}",
//            session.ImpersonatorUserName,
//            session.ImpersonatedUserName,
//            session.DurationSeconds,
//            session.ActionsPerformedCount);

//        return new EndImpersonationResponse
//        {
//            OriginalToken = originalToken,
//            SessionDurationSeconds = session.DurationSeconds ?? 0,
//            ActionsPerformed = session.ActionsPerformedCount
//        };
//    }

//    /// <summary>
//    /// Get all active impersonation sessions (UC-19.3)
//    /// </summary>
//    public async Task<List<ActiveImpersonationSessionDto>> GetActiveSessionsAsync(CancellationToken cancellationToken = default)
//    {
//        var sessions = await _sessionRepository.GetActiveSessionsAsync(cancellationToken);

//        var result = new List<ActiveImpersonationSessionDto>();
//        foreach (var session in sessions)
//        {
//            var impersonator = await _userManager.FindByIdAsync(session.ImpersonatorUserId.ToString());
//            var impersonated = await _userManager.FindByIdAsync(session.ImpersonatedUserId.ToString());
//            var impersonatorRoles = impersonator != null ? await _userManager.GetRolesAsync(impersonator) : Enumerable.Empty<string>();
//            var impersonatedRoles = impersonated != null ? await _userManager.GetRolesAsync(impersonated) : Enumerable.Empty<string>();

//            result.Add(new ActiveImpersonationSessionDto
//            {
//                SessionId = session.Id,
//                ImpersonatorUserId = session.ImpersonatorUserId,
//                ImpersonatorUserName = session.ImpersonatorUserName,
//                ImpersonatorEmail = impersonator?.Email,
//                ImpersonatorRole = impersonatorRoles.FirstOrDefault(),
//                ImpersonatedUserId = session.ImpersonatedUserId,
//                ImpersonatedUserName = session.ImpersonatedUserName,
//                ImpersonatedEmail = impersonated?.Email,
//                ImpersonatedRole = impersonatedRoles.FirstOrDefault(),
//                StartTime = session.StartTime,
//                OriginalUserIpAddress = session.OriginalUserIpAddress,
//                ActionsPerformedCount = session.ActionsPerformedCount
//            });
//        }

//        return result;
//    }

//    /// <summary>
//    /// Terminate an active session (admin override) (UC-19.4)
//    /// </summary>
//    public async Task TerminateSessionAsync(
//        Guid sessionId,
//        Guid terminatedBy,
//        string? reason = null,
//        CancellationToken cancellationToken = default)
//    {
//        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
//        if (session == null)
//        {
//            throw new InvalidOperationException("Session not found.");
//        }

//        if (!session.IsActive)
//        {
//            return; // Already terminated
//        }

//        var terminatingUser = await _userManager.FindByIdAsync(terminatedBy.ToString());
//        var terminationReason = reason ?? "Admin Override";

//        await TerminateSessionInternalAsync(session, terminatedBy, terminationReason, terminatingUser?.UserName ?? terminatingUser.ToString());

//        _logger.LogWarning(
//            "Impersonation session terminated by admin: {TerminatedBy} -> Session {SessionId} ({Impersonator} as {Impersonated}), Reason: {Reason}",
//            terminatingUser?.UserName,
//            sessionId,
//            session.ImpersonatorUserName,
//            session.ImpersonatedUserName,
//            terminationReason);
//    }

//    /// <summary>
//    /// Search for users by username/email (UC-19.5)
//    /// </summary>
//    public async Task<List<UserSearchResult>> SearchUsersAsync(
//        string searchTerm,
//        Guid requesterId,
//        CancellationToken cancellationToken = default)
//    {
//        if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Length < 3)
//        {
//            return new List<UserSearchResult>();
//        }

//        var requester = await _userManager.FindByIdAsync(requesterId.ToString());
//        if (requester == null)
//        {
//            return new List<UserSearchResult>();
//        }

//        var requesterRoles = await _userManager.GetRolesAsync(requester);
//        var isAdmin = requesterRoles.Contains("Super Admin") || requesterRoles.Contains("Admin");

//        // Search users by username or email
//        var users = _userManager.Users
//            .Where(u => u.IsActive && (u.UserName != null && u.UserName.Contains(searchTerm) ||
//                                       u.Email != null && u.Email.Contains(searchTerm)))
//            .Take(10);

//        var result = new List<UserSearchResult>();
//        foreach (var user in users)
//        {
//            var userRoles = await _userManager.GetRolesAsync(user);
//            var canImpersonate = isAdmin &&
//                                  (requesterRoles.Contains("Super Admin") || !userRoles.Contains("Super Admin"));

//            result.Add(new UserSearchResult
//            {
//                Id = user.Id,
//                Username = user.UserName ?? user.Email,
//                Email = user.Email ?? string.Empty,
//                FullName = user.FullName,
//                Role = userRoles.FirstOrDefault(),
//                IsActive = user.IsActive,
//                CanImpersonate = canImpersonate && user.IsActive
//            });
//        }

//        return result;
//    }

//    /// <summary>
//    /// Get impersonation history (UC-19.6)
//    /// </summary>
//    public async Task<List<ImpersonationSessionHistoryDto>> GetHistoryAsync(
//        ImpersonationHistoryFilter filter,
//        CancellationToken cancellationToken = default)
//    {
//        var sessions = await _sessionRepository.GetHistoryAsync(
//            filter.ImpersonatorId,
//            filter.ImpersonatedId,
//            filter.StartDate,
//            filter.EndDate,
//            cancellationToken);

//        return sessions.Select(s => new ImpersonationSessionHistoryDto
//        {
//            SessionId = s.Id,
//            ImpersonatorUserId = s.ImpersonatorUserId,
//            ImpersonatorUserName = s.ImpersonatorUserName,
//            ImpersonatedUserId = s.ImpersonatedUserId,
//            ImpersonatedUserName = s.ImpersonatedUserName,
//            StartTime = s.StartTime,
//            EndTime = s.EndTime,
//            DurationSeconds = s.DurationSeconds,
//            IsActive = s.IsActive,
//            ActionsPerformedCount = s.ActionsPerformedCount,
//            TerminationReason = s.TerminationReason,
//            TerminatedByName = s.TerminatedByName
//        }).ToList();
//    }

//    /// <summary>
//    /// Get current impersonation status
//    /// </summary>
//    public async Task<ImpersonationStatusDto> GetStatusAsync(
//        Guid userId,
//        CancellationToken cancellationToken = default)
//    {
//        var session = await _sessionRepository.GetActiveSessionByImpersonatorAsync(userId, cancellationToken);

//        if (session == null)
//        {
//            return new ImpersonationStatusDto { IsImpersonating = false };
//        }

//        var impersonatedUser = await _userManager.FindByIdAsync(session.ImpersonatedUserId.ToString());
//        var impersonatedRoles = impersonatedUser != null
//            ? await _userManager.GetRolesAsync(impersonatedUser)
//            : Enumerable.Empty<string>();

//        return new ImpersonationStatusDto
//        {
//            IsImpersonating = true,
//            SessionId = session.Id,
//            OriginalUserId = session.ImpersonatorUserId,
//            OriginalUserName = session.ImpersonatorUserName,
//            SessionStartTime = session.StartTime,
//            SessionExpiresAt = session.StartTime.AddHours(_settings.MaxSessionDurationHours),
//            CurrentUser = new ImpersonatedUserInfo
//            {
//                Id = impersonatedUser?.Id ?? Guid.Empty,
//                Username = impersonatedUser?.UserName ?? string.Empty,
//                Email = impersonatedUser?.Email ?? string.Empty,
//                FullName = impersonatedUser?.FullName,
//                Roles = impersonatedRoles.ToList()
//            }
//        };
//    }

//    /// <summary>
//    /// Get impersonation settings (UC-19.8)
//    /// </summary>
//    public Task<ImpersonationSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
//    {
//        return Task.FromResult(new ImpersonationSettingsDto
//        {
//            MaxSessionDurationHours = _settings.MaxSessionDurationHours,
//            WarningThresholdMinutes = _settings.WarningThresholdMinutes,
//            AllowSessionExtension = _settings.AllowSessionExtension,
//            MaxExtensionsPerSession = _settings.MaxExtensionsPerSession,
//            RequireConfirmation = _settings.RequireConfirmation,
//            LogAllActions = _settings.LogAllActions,
//            NotifyImpersonatedUser = _settings.NotifyImpersonatedUser,
//            IpRestrictionEnabled = _settings.IpRestrictionEnabled,
//            LogRetentionDays = _settings.LogRetentionDays
//        });
//    }

//    /// <summary>
//    /// Update impersonation settings (UC-19.8)
//    /// </summary>
//    public Task UpdateSettingsAsync(ImpersonationSettingsDto settings, CancellationToken cancellationToken = default)
//    {
//        // This would update settings in database or configuration
//        // For now, we'll log the change
//        _logger.LogInformation("Impersonation settings updated: {Settings}", System.Text.Json.JsonSerializer.Serialize(settings));
//        return Task.CompletedTask;
//    }

//    /// <summary>
//    /// Validate if a user can impersonate another user
//    /// </summary>
//    public async Task<(bool CanImpersonate, string? Reason)> ValidateImpersonationAsync(
//        Guid impersonatorId,
//        Guid targetUserId,
//        CancellationToken cancellationToken = default)
//    {
//        // Same user
//        if (impersonatorId == targetUserId)
//        {
//            return (false, "Cannot impersonate yourself.");
//        }

//        var impersonator = await _userManager.FindByIdAsync(impersonatorId.ToString());
//        var targetUser = await _userManager.FindByIdAsync(targetUserId.ToString());

//        if (impersonator == null)
//        {
//            return (false, "Impersonator user not found.");
//        }

//        if (targetUser == null)
//        {
//            return (false, "Target user not found.");
//        }

//        var impersonatorRoles = await _userManager.GetRolesAsync(impersonator);
//        var targetRoles = await _userManager.GetRolesAsync(targetUser);

//        // Check if impersonator has impersonation permission
//        if (!impersonatorRoles.Any(r => r is "Super Admin" or "Admin"))
//        {
//            return (false, "You do not have permission to impersonate users.");
//        }

//        // Admin cannot impersonate Super Admin
//        if (impersonatorRoles.Contains("Admin") && targetRoles.Contains("Super Admin"))
//        {
//            return (false, "Admin cannot impersonate Super Admin users.");
//        }

//        // Target user must be active
//        if (!targetUser.IsActive)
//        {
//            return (false, "Cannot impersonate inactive user.");
//        }

//        return (true, null);
//    }

//    /// <summary>
//    /// Increment action counter (UC-19.7)
//    /// </summary>
//    public async Task IncrementActionCountAsync(Guid sessionId, CancellationToken cancellationToken = default)
//    {
//        await _sessionRepository.IncrementActionsCountAsync(sessionId, cancellationToken);
//    }

//    /// <summary>
//    /// Generate JWT token with impersonation claims
//    /// </summary>
//    private async Task<string> GenerateImpersonationTokenAsync(ApplicationUser targetUser, Guid sessionId, Guid impersonatorId)
//    {
//        var key = Encoding.UTF8.GetBytes("YourSuperSecretKeyWith32CharactersLength!!");
//        var issuer = "IIROSAApi";
//        var audience = "IIROSAClient";

//        var roles = await _userManager.GetRolesAsync(targetUser);

//        var claims = new List<Claim>
//        {
//            new Claim(ClaimTypes.NameIdentifier, targetUser.Id.ToString()),
//            new Claim(ClaimTypes.Name, targetUser.UserName ?? string.Empty),
//            new Claim(ClaimTypes.Email, targetUser.Email ?? string.Empty),
//            new Claim("FullName", targetUser.FullName ?? string.Empty),
//            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//            // Impersonation claims
//            new Claim("ImpersonationSessionId", sessionId.ToString()),
//            new Claim("OriginalUserId", impersonatorId.ToString()),
//            new Claim("IsImpersonating", "true")
//        };

//        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(claims),
//            Expires = DateTime.UtcNow.AddHours(_settings.MaxSessionDurationHours),
//            Issuer = issuer,
//            Audience = audience,
//            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
//        };

//        var tokenHandler = new JwtSecurityTokenHandler();
//        var token = tokenHandler.CreateToken(tokenDescriptor);
//        return tokenHandler.WriteToken(token);
//    }

//    /// <summary>
//    /// Internal method to terminate a session
//    /// </summary>
//    private async Task TerminateSessionInternalAsync(
//        ImpersonationSession session,
//        Guid? terminatedBy,
//        string reason,
//        string? terminatedByName = null)
//    {
//        session.EndTime = DateTime.UtcNow;
//        session.IsActive = false;
//        session.TerminatedBy = terminatedBy;
//        session.TerminatedByName = terminatedByName;
//        session.TerminationReason = reason;
//        session.UpdatedOn = DateTime.UtcNow;

//        _sessionRepository.Update(session);
//        await _unitOfWork.SaveChangesAsync();
//    }
//}

///// <summary>
///// Impersonation settings configuration
///// </summary>
//public class ImpersonationSettings
//{
//    public int MaxSessionDurationHours { get; set; } = 8;
//    public int WarningThresholdMinutes { get; set; } = 15;
//    public bool AllowSessionExtension { get; set; } = true;
//    public int MaxExtensionsPerSession { get; set; } = 2;
//    public bool RequireConfirmation { get; set; } = true;
//    public bool LogAllActions { get; set; } = true;
//    public bool NotifyImpersonatedUser { get; set; } = false;
//    public bool IpRestrictionEnabled { get; set; } = false;
//    public int LogRetentionDays { get; set; } = 365;
//}
