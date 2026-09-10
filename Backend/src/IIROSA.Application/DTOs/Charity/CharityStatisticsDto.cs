namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Register statistics band shown above the all-charities grid (UC-3.10).
/// Counts follow the caller's scope: a charity caller sees their own record only and a
/// country-pinned head-office caller their country, because the service resolves the scope
/// with the same ladder as the list itself.
/// </summary>
public class CharityStatisticsDto
{
    public int TotalCharities { get; set; }
    public int ActiveCharities { get; set; }
    public int InactiveCharities { get; set; }
    public int LockedCharities { get; set; }
    public int ReceivingDonations { get; set; }
    /// <summary>Charities registered since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
    /// <summary>Charity counts per country, highest first. Empty when the scope is a single charity.</summary>
    public List<CharityCountryStatisticsDto> ByCountry { get; set; } = new();
}

/// <summary>
/// One row of the by-country breakdown — bilingual per the platform naming rule,
/// the client picks by current language.
/// </summary>
public class CharityCountryStatisticsDto
{
    public int CountryId { get; set; }
    public string? NameAr { get; set; }
    public string? NameEn { get; set; }
    public int Count { get; set; }
}
