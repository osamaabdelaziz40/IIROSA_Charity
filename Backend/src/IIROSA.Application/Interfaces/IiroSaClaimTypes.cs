namespace IIROSA.Application.Interfaces;

/// <summary>
/// Claim types this system adds to the access token on top of the standard ASP.NET Identity set.
/// Tenancy is resolved from these claims server-side; it is never taken from the request body or
/// the query string, because the caller controls those.
/// </summary>
public static class IiroSaClaimTypes
{
    /// <summary>
    /// The charity the caller belongs to. Absent for head-office users, who are not scoped to a
    /// single charity.
    /// </summary>
    public const string CharityId = "charityId";

    /// <summary>
    /// The country the caller operates in.
    /// </summary>
    public const string CountryId = "countryId";
}
