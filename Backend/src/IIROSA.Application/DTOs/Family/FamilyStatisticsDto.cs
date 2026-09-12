namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Register statistics band shown above the family list pages (families, refugee register,
/// housing register). Counts follow the caller's scope exactly like the list itself: a
/// Charity-role caller sees its own register only, everyone else the whole register, and the
/// optional familyType discriminator narrows the counts to that one register.
/// </summary>
public class FamilyStatisticsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Inactive { get; set; }
    /// <summary>Families registered since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
