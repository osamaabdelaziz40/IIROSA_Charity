namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Result of checking whether a charity name may still be used (UC-CHR-02).
/// </summary>
/// <remarks>
/// Returned instead of a bare boolean so the client can tell which name the answer refers to. The
/// name field is typed into while requests are in flight, and a stale reply carrying no name is
/// indistinguishable from a current one.
/// </remarks>
public class CharityNameAvailabilityDto
{
    /// <summary>The name that was checked, echoed back.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// True when no other charity holds this name. Uniqueness is register-wide, matching the rule
    /// <c>CreateCharityAsync</c> and <c>UpdateCharityAsync</c> enforce on save.
    /// </summary>
    public bool IsAvailable { get; set; }
}
