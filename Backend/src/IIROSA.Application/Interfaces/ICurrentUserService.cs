namespace IIROSA.Application.Interfaces;

/// <summary>
/// The caller behind the current request, resolved from the access token.
/// </summary>
/// <remarks>
/// Declared in the Application layer and implemented in the API layer so that application services
/// can scope their queries without taking a dependency on ASP.NET Core, per the Clean Architecture
/// rule that inner layers never reference outer ones.
///
/// Every value here comes from signed token claims. None of it is ever read from a request body or
/// query string, because the caller controls those.
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>The caller's user id, or null when the request is unauthenticated.</summary>
    Guid? UserId { get; }

    /// <summary>
    /// The charity the caller belongs to, or null for a head-office user who is not bound to one.
    /// </summary>
    Guid? CharityId { get; }

    /// <summary>The country the caller operates in, or null when the token carries none.</summary>
    int? CountryId { get; }

    /// <summary>True when the caller is authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// True when the caller holds a head-office role and may therefore look across charities.
    /// A caller bound to a charity is never treated as head office, whatever roles they hold.
    /// </summary>
    bool IsHeadOffice { get; }

    /// <summary>True when the caller holds the named role.</summary>
    bool IsInRole(string role);
}
