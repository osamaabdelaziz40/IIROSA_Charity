using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Dtos;
using Microsoft.AspNetCore.Identity;

namespace IIROSA.Application.Services;

/// <summary>
/// JWT Token generation service implementation
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<ApplicationUser> _userManager;

    // Token expiration settings (in minutes)
    private const int DefaultAccessTokenExpirationMinutes = 30;
    private const int DefaultRefreshTokenExpirationDays = 7;

    public TokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
    {
        _configuration = configuration;
        _userManager = userManager;
    }

    public async Task<string> GenerateTokenAsync(ApplicationUser user)
    {
        var (token, _) = await GenerateTokenInternalAsync(user);
        return token;
    }

    public Task<string> GenerateTokenAsync(UserDto userDto)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyWith32CharactersLength!!");
        var issuer = _configuration["Jwt:Issuer"] ?? "IIROSAApi";
        var audience = _configuration["Jwt:Audience"] ?? "IIROSAClient";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userDto.Id?.ToString() ?? string.Empty),
            new Claim(ClaimTypes.Name, userDto.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, userDto.Email ?? string.Empty),
            new Claim("FullName", userDto.FullName ?? string.Empty)
        };

        // Add roles from UserDto
        if (userDto.RoleNames != null)
        {
            claims.AddRange(userDto.RoleNames.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // CRITICAL: Disable claim type mapping to preserve full claim type URIs
        // This must be done BEFORE creating the token handler instance
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(tokenHandler.WriteToken(token));
    }

    /// <summary>
    /// Generate a cryptographically secure random refresh token
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    /// <summary>
    /// Generate both access and refresh tokens for a user
    /// </summary>
    public async Task<(string accessToken, string refreshToken, DateTime expiration)> GenerateTokenPairAsync(ApplicationUser user)
    {
        var (accessToken, expiration) = await GenerateTokenInternalAsync(user);
        var refreshToken = GenerateRefreshToken();
        return (accessToken, refreshToken, expiration);
    }

    /// <summary>
    /// Generate both access and refresh tokens for UserDto
    /// </summary>
    public async Task<(string accessToken, string refreshToken, DateTime expiration)> GenerateTokenPairAsync(UserDto userDto)
    {
        var accessToken = await GenerateTokenAsync(userDto);
        var expiration = GetAccessTokenExpiration();
        var refreshToken = GenerateRefreshToken();
        return (accessToken, refreshToken, expiration);
    }

    /// <summary>
    /// Get the expiration time for access tokens
    /// </summary>
    public DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes());
    }

    /// <summary>
    /// Get the expiration time for refresh tokens
    /// </summary>
    public DateTime GetRefreshTokenExpiration(int? daysValid = null)
    {
        var days = daysValid ?? DefaultRefreshTokenExpirationDays;
        return DateTime.UtcNow.AddDays(days);
    }

    /// <summary>
    /// Internal method to generate JWT token with expiration
    /// </summary>
    private async Task<(string token, DateTime expiration)> GenerateTokenInternalAsync(ApplicationUser user)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKeyWith32CharactersLength!!");
        var issuer = _configuration["Jwt:Issuer"] ?? "IIROSAApi";
        var audience = _configuration["Jwt:Audience"] ?? "IIROSAClient";

        var expiration = GetAccessTokenExpiration();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("FullName", user.FullName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // CRITICAL: Disable claim type mapping to preserve full claim type URIs
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);

        return (tokenString, expiration);
    }

    /// <summary>
    /// Get access token expiration minutes from configuration
    /// </summary>
    private int GetAccessTokenExpirationMinutes()
    {
        if (int.TryParse(_configuration["Jwt:ExpirationInMinutes"], out var minutes))
        {
            return minutes;
        }
        return DefaultAccessTokenExpirationMinutes;
    }
}
