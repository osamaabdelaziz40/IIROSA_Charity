namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Orphan search filter (UC-ORP-02 find by name, UC-ORP-03 coding worklist, UC-ORP-07 resolve name from code).
/// One filter drives the shared <c>GET /api/Families/orphans</c> endpoint: a plain search (all roles)
/// and — when <see cref="CodingStatus"/> is supplied — the HQ-only coding worklist.
/// </summary>
public class OrphanSearchFilterDto
{
    /// <summary>
    /// Free-text term matched against the orphan's full name and code
    /// (a code typed here resolves back to the name — UC-ORP-07).
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// HQ narrowing: operate on this charity's orphans. Empty = all charities (HQ only).
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// "Pending" = uncoded (empty <c>Orphan.Code</c> — the §13.D definition, not SponsorshipStatus);
    /// "Coded" = has a code. Supplying this value makes the query the coding worklist (HQ only).
    /// </summary>
    public string? CodingStatus { get; set; }

    /// <summary>
    /// Sort field: FullName (default), Code, DateOfBirth, CharityName.
    /// </summary>
    public string? SortBy { get; set; } = "FullName";

    public bool SortDescending { get; set; }

    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 20 by default — the right shape for the UC-ORP-02 type-ahead; the worklist pages normally.
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Orphan lookup row (UC-ORP-02/03/07). Carries the judgement data of §13.S.2:
/// orphan name, father name, mother name, charity name, code, date of birth.
/// </summary>
public class OrphanLookupDto
{
    public Guid OrphanId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? CharityName { get; set; }
    public Guid? CharityId { get; set; }
    public Guid? FamilyId { get; set; }
    public string? FamilyCode { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? NationalId { get; set; }
    public string? Phone { get; set; }
    public string? SponsorshipStatus { get; set; }
    public string? EducationLevelName { get; set; }
    public string? HealthStatusName { get; set; }
}

/// <summary>
/// UC-ORP-01 — check whether an orphan may be added (national ID + charity state verdict).
/// </summary>
public class OrphanEligibilityCheckDto
{
    /// <summary>
    /// The national ID typed on the orphan form — checked for prior registration in scope.
    /// Optional since P3: with only a family id given, the check reduces to the family gate.
    /// </summary>
    public string? NationalId { get; set; }

    /// <summary>
    /// The family the orphan form is opened on — when given, the family must exist, be active
    /// and sit inside the resolved charity scope (P3).
    /// </summary>
    public Guid? FamilyId { get; set; }

    /// <summary>
    /// HQ override: check within this charity's register. Charity callers are pinned server-side.
    /// </summary>
    public Guid? CharityId { get; set; }
}

/// <summary>
/// Verdict of the UC-ORP-01 eligibility check. A refusal names the offending field and reason so
/// the form can flag the input; a pass carries no reason.
/// </summary>
public class OrphanEligibilityDto
{
    public bool CanBeAdded { get; set; }

    /// <summary>The form field to flag: nationalId, charity or family.</summary>
    public string? Field { get; set; }

    /// <summary>
    /// Machine key of the refusal (P16) — an i18n key under <c>orphanCoding.reasons.*</c>; the
    /// translated label is composed client-side. A pass carries none.
    /// </summary>
    public string? ReasonCode { get; set; }

    public string? ExistingOrphanName { get; set; }
}

/// <summary>
/// UC-ORP-05 — verify a sponsorship code is not already used (BR-07: unique within the charity).
/// </summary>
public class OrphanCodeCheckFilterDto
{
    public string? Code { get; set; }

    /// <summary>
    /// HQ override: uniqueness is judged within this charity. Charity callers are pinned server-side.
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// The orphan being re-coded (worklist edit) — its own current code never clashes with itself.
    /// </summary>
    public Guid? ExcludeOrphanId { get; set; }
}

/// <summary>
/// Verdict of the UC-ORP-05 code check: available, or already held (with holder labels for the message).
/// </summary>
public class OrphanCodeCheckDto
{
    public bool IsAvailable { get; set; }
    public string? ExistingOrphanName { get; set; }
    public string? ExistingCharityName { get; set; }
}

/// <summary>
/// UC-ORP-06 — assign a sponsorship code to an orphan.
/// </summary>
public class AssignOrphanCodeDto
{
    public Guid OrphanId { get; set; }
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// UC-ORP-10 — check a phone number is not duplicated.
/// </summary>
public class PhoneCheckFilterDto
{
    /// <summary>
    /// The number as typed — trimmed before compare.
    /// </summary>
    public string? Number { get; set; }

    /// <summary>
    /// Accepted for wire compatibility with the legacy per-type check, but the match is
    /// type-agnostic: no phone column in the Domain carries a type today (recorded simplification).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// HQ override: match within this charity's beneficiaries. Charity callers are pinned server-side.
    /// </summary>
    public Guid? CharityId { get; set; }
}

/// <summary>
/// Verdict of the UC-ORP-10 phone check: the offending field is flagged with the existing holder's label.
/// </summary>
public class PhoneCheckDto
{
    public bool IsDuplicate { get; set; }

    /// <summary>
    /// The holder as structured data (P16) — family code, holder name and type key; the
    /// translated label is composed client-side, not server-side in English.
    /// </summary>
    public string? HolderFamilyCode { get; set; }
    public string? HolderName { get; set; }

    /// <summary>family | father | mother | provider | orphan — the i18n key suffix for the type word.</summary>
    public string? HolderType { get; set; }
}
