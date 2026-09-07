namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Lightweight orphan identity for the by-code lookup that prefaces report entry
/// (UC-ORR-02, epic 9). Carries enough to prefill the report header and confirm
/// the charity scope before the form opens.
/// </summary>
public class OrphanLookupDto
{
    public Guid OrphanId { get; set; }

    /// <summary>The sponsorship code the caller typed — empty when the orphan is uncoded.</summary>
    public string? Code { get; set; }

    public string? FullName { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public DateTime? BirthDate { get; set; }

    /// <summary>Family reference for the report header (UC-ORR-02 prefill).</summary>
    public Guid? FamilyId { get; set; }
    public string? FamilyCode { get; set; }

    /// <summary>The living معيل from the family register, when one is recorded.</summary>
    public string? GuardianName { get; set; }

    /// <summary>Report counters so the entry screen can warn about an existing report for the month.</summary>
    public int TotalReports { get; set; }
    public int PendingReports { get; set; }

    /// <summary>True when the orphan carries a sponsorship code — the gate for report entry (BR-09).</summary>
    public bool Coded => !string.IsNullOrWhiteSpace(Code);
}
