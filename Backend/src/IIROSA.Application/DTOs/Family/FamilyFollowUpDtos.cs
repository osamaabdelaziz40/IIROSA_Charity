namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Typed filter for the family follow-up report (UC-FAM-11 متابعة إدخالات الأسر) — one calendar
/// day of register activity, optionally narrowed to a charity by HQ. A Charity-role caller is
/// scoped server-side to its own charity regardless of the charity filter it sends.
/// </summary>
public class FamilyFollowUpFilterDto
{
    /// <summary>Which day's entries to list (required, not in the future)</summary>
    public DateTime Date { get; set; }

    /// <summary>HQ-only charity filter; overridden for Charity-role callers</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number (default 1)</summary>
    public int? PageNumber { get; set; }

    /// <summary>Page size (default 10, clamped to 100; the print path may request up to 5000
    /// through the internal ceiling)</summary>
    public int? PageSize { get; set; }
}

/// <summary>
/// One follow-up row — a family file that was created or updated on the report date. Pure
/// projection over the inherited audit columns (CreatedOn/By, UpdatedOn/By); nothing is stored.
/// </summary>
public class FamilyFollowUpListDto
{
    /// <summary>The family the activity concerns</summary>
    public Guid FamilyId { get; set; }

    /// <summary>The family register code</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>The family head as registered on the file</summary>
    public string HeadOfFamily { get; set; } = string.Empty;

    /// <summary>The charity whose register the family belongs to</summary>
    public string? CharityName { get; set; }

    /// <summary>"Created" or "Updated" — created wins when both happened on the day</summary>
    public string ChangeKind { get; set; } = string.Empty;

    /// <summary>Who made the change (audit user name)</summary>
    public string? ChangedBy { get; set; }

    /// <summary>When the change was made (creation time, or the last update time)</summary>
    public DateTime ChangedOn { get; set; }

    /// <summary>How many of the family's orphans were created/updated the same day</summary>
    public int OrphansTouched { get; set; }
}
