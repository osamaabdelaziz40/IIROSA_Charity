using System.Security.Claims;
using IIROSA.Application.Interfaces;

namespace IIROSA.Api.Services;

/// <summary>
/// Resolves the current caller from the access token attached to the request.
/// </summary>
/// <remarks>
/// Lives in the API layer because it reads <see cref="IHttpContextAccessor"/>; the Application
/// layer consumes only the <see cref="ICurrentUserService"/> abstraction.
/// </remarks>
public class CurrentUserService : ICurrentUserService
{
    /// <summary>
    /// Roles that act on behalf of head office and may therefore look across charities.
    /// </summary>
    private static readonly string[] HeadOfficeRoles = { "SuperAdmin", "Admin" };

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId =>
        Guid.TryParse(Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id)
            ? id
            : null;

    public Guid? CharityId =>
        Guid.TryParse(Principal?.FindFirst(IiroSaClaimTypes.CharityId)?.Value, out var id)
            ? id
            : null;

    public int? CountryId =>
        int.TryParse(Principal?.FindFirst(IiroSaClaimTypes.CountryId)?.Value, out var id)
            ? id
            : null;

    /// <summary>
    /// A caller carrying a charity claim is bound to that charity and is never head office, even
    /// if they also hold an HQ role. Role alone does not widen scope.
    /// </summary>
    public bool IsHeadOffice =>
        IsAuthenticated && !CharityId.HasValue && HeadOfficeRoles.Any(IsInRole);

    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
}
