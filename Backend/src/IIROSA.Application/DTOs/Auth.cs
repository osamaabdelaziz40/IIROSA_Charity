namespace IIROSA.Application.DTOs;

/// <summary>
/// Login request DTO
/// </summary>
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
}

/// <summary>
/// Login response DTO
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public LoginUser User { get; set; } = new();
}

/// <summary>
/// Login user details
/// </summary>
public class LoginUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public IList<string> Roles { get; set; } = new List<string>();
}

/// <summary>
/// Register request DTO
/// </summary>
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token request DTO
/// </summary>
public class RefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}

/// <summary>
/// Refresh token response DTO
/// </summary>
public class RefreshTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public int ExpiresInMinutes { get; set; }
}
