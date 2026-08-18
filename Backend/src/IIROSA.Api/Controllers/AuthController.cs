using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Framework.Identity.Data.Services;
using Framework.Identity.Data.Services.Interfaces;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using IIROSA.Application.Services;
using IIROSA.Application.DTOs;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Authentication controller - Uses Framework.Identity UserAppService
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AuthController : ApiController
{
    private readonly IUserAppService _userAppService;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthController(
        IUserAppService userAppService,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthController> logger) : base(logger)
    {
        _userAppService = userAppService;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

    /// <summary>
    /// Login user and generate JWT token with refresh token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _userAppService.Login(loginDto);

        if (result.Success && result.Value != null)
        {
            var (accessToken, refreshToken, expiration) = await _tokenService.GenerateTokenPairAsync(result.Value);

            // Store refresh token in database
            var userId = result.Value.Id ?? Guid.Empty;
            var refreshTokenEntity = await _refreshTokenService.CreateRefreshTokenAsync(
                userId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString());

            return Ok(new LoginResponse
            {
                Token = accessToken,
                RefreshToken = refreshTokenEntity.Token,
                Expiration = expiration,
                User = new LoginUser
                {
                    Id = result.Value.Id?.ToString() ?? string.Empty,
                    Username = result.Value.UserName ?? string.Empty,
                    Email = result.Value.Email ?? string.Empty,
                    FullName = result.Value.FullName ?? string.Empty,
                    Roles = result.Value.RoleNames?.ToList() ?? new List<string>()
                }
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Register new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserRegister model)
    {
        var result = await _userAppService.Register(model);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new { success = false, message = "Refresh token is required" });
        }

        // Validate the refresh token
        var (isValid, refreshTokenEntity) = await _refreshTokenService.ValidateRefreshTokenAsync(request.RefreshToken);

        if (!isValid || refreshTokenEntity == null)
        {
            return Unauthorized(new { success = false, message = "Invalid or expired refresh token" });
        }

        // Get the user
        var user = refreshTokenEntity.User;
        if (user == null)
        {
            return Unauthorized(new { success = false, message = "User not found" });
        }

        // Generate new token pair
        var (newAccessToken, newRefreshToken, expiration) = await _tokenService.GenerateTokenPairAsync(user);

        // Mark old refresh token as used and link to new one
        await _refreshTokenService.MarkAsUsedAsync(refreshTokenEntity, newRefreshToken);

        // Create new refresh token entity
        var newRefreshTokenEntity = await _refreshTokenService.CreateRefreshTokenAsync(
            user.Id,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            Request.Headers["User-Agent"].ToString());

        // Update the new refresh token value
        newRefreshToken = newRefreshTokenEntity.Token;

        var expiresInMinutes = (int)(expiration - DateTime.UtcNow).TotalMinutes;

        return Ok(new RefreshTokenResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            Expiration = expiration,
            ExpiresInMinutes = expiresInMinutes
        });
    }

    /// <summary>
    /// Logout user and revoke refresh tokens
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userId, out var userGuid))
        {
            // Revoke all refresh tokens for this user
            await _refreshTokenService.RevokeAllUserTokensAsync(userGuid, "User logged out");
        }

        return Ok(new { success = true, message = "Logged out successfully" });
    }

    /// <summary>
    /// Validate current token and check expiration
    /// </summary>
    [HttpPost("validate-token")]
    [Authorize]
    public IActionResult ValidateToken()
    {
        var expiration = User.FindFirst("exp")?.Value;
        if (long.TryParse(expiration, out var expSeconds))
        {
            var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
            var minutesRemaining = (int)(expirationDate - DateTime.UtcNow).TotalMinutes;

            return Ok(new
            {
                success = true,
                isValid = minutesRemaining > 0,
                expiresAt = expirationDate,
                minutesRemaining = Math.Max(0, minutesRemaining),
                shouldShowWarning = minutesRemaining > 0 && minutesRemaining <= 5
            });
        }

        return BadRequest(new { success = false, message = "Invalid token" });
    }
}

/// <summary>
/// Logout request DTO
/// </summary>
public class LogoutRequest
{
    public string? RefreshToken { get; set; }
}
