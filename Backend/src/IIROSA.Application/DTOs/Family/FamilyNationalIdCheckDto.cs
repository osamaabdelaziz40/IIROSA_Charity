namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// UC-SYS-12 — family/guardian-level national-id uniqueness check request (typed binding;
/// the legacy untyped JObject shape is a recorded defect, prd.md §7).
/// </summary>
public class CheckFamilyNationalIdDto
{
    /// <summary>The national ID typed on a family/guardian form — checked across every person holder.</summary>
    public string NationalId { get; set; } = string.Empty;

    /// <summary>
    /// The family being edited — its own holders never count as a clash (re-saving an
    /// unchanged person must not self-report).
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// HQ override: check within this charity's register. Charity callers are pinned server-side
    /// to their claim; HQ without a charityId is refused with a field error.
    /// </summary>
    public Guid? CharityId { get; set; }
}

/// <summary>
/// Verdict of the UC-SYS-12 check. A clash names the holder (person + family code + holder kind)
/// so the user can decide rather than guess; a pass carries only IsUnique = true.
/// </summary>
public class FamilyNationalIdCheckResultDto
{
    public bool IsUnique { get; set; }

    /// <summary>Full name of the person already holding the id (null when unique).</summary>
    public string? HolderName { get; set; }

    /// <summary>Family code of the family the holder belongs to (null when unique).</summary>
    public string? HolderFamilyCode { get; set; }

    /// <summary>Holder kind: Father, Mother, Provider, Relative or Orphan (null when unique).</summary>
    public string? HolderType { get; set; }
}
