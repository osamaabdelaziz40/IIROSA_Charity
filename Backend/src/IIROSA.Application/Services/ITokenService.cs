using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;

namespace IIROSA.Application.Services;

/// <summary>
/// JWT Token generation service
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    Task<string> GenerateTokenAsync(ApplicationUser user);

    /// <summary>
    /// Generate JWT token for user DTO
    /// </summary>
    Task<string> GenerateTokenAsync(UserDto userDto);

    /// <summary>
    /// Generate a secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Generate both access and refresh tokens
    /// </summary>
    Task<(string accessToken, string refreshToken, DateTime expiration)> GenerateTokenPairAsync(ApplicationUser user);

    /// <summary>
    /// Generate both access and refresh tokens for UserDto
    /// </summary>
    Task<(string accessToken, string refreshToken, DateTime expiration)> GenerateTokenPairAsync(UserDto userDto);

    /// <summary>
    /// Get the expiration time for access tokens
    /// </summary>
    DateTime GetAccessTokenExpiration();

    /// <summary>
    /// Get the expiration time for refresh tokens (default 7 days)
    /// </summary>
    DateTime GetRefreshTokenExpiration(int? daysValid = null);
}
