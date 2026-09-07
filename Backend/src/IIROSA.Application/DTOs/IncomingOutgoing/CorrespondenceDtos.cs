namespace IIROSA.Application.DTOs.IncomingOutgoing;

// ========== Shared catalogue / lookup option DTOs (epic 16) ==========

/// <summary>
/// A letter status option (epic 16 — the spec's tri-state: معلق / تم الرد / تم عمل اللازم).
/// The stored value is the Arabic term; Id is that value so the SPA can bind it directly.
/// </summary>
public class CorrespondenceStatusDto
{
    public string Id { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string Color { get; set; } = "secondary";
}

/// <summary>
/// An outgoing category option (epic 16, UC-COR-17) — table-backed, bilingual.
/// </summary>
public class OutgoingCategoryOptionDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

/// <summary>
/// The advisory next serial (epic 16, UC-COR-03 / UC-COR-12). SerialTxt is the plain
/// zero-padded form the register displays; the definitive serial is re-derived inside
/// the create transaction.
/// </summary>
public class NextSerialDto
{
    public int Serial { get; set; }
    public string SerialTxt { get; set; } = string.Empty;
}

// ========== Employee attachment (epic 16, UC-COR-09) ==========

public class EmployeeOptionDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Email { get; set; }
}

/// <summary>
/// The two lists of the §21.S.3 screen: employees already attached to the letter and
/// active employees available to attach.
/// </summary>
public class IncomingEmployeesDto
{
    public List<EmployeeOptionDto> Attached { get; set; } = new();
    public List<EmployeeOptionDto> Available { get; set; } = new();
}

public class AttachEmployeeDto
{
    public Guid UserId { get; set; }
}

// ========== Orphan report attachment (epic 16, UC-COR-18) ==========

/// <summary>
/// Flat orphan projection for the §21.S.6 grids — no client-side joins.
/// Guarantor and kinship ride with the family register (HeadOfFamily / ProviderType).
/// </summary>
public class OrphanOptionDto
{
    public Guid OrphanId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? GuarantorName { get; set; }
    public string? Kinship { get; set; }
}

/// <summary>
/// The two grids of the §21.S.6 screen: orphans attached to the letter and the letter's
/// charity's orphans not attached to any letter (BR-26 + BR-27).
/// </summary>
public class OutgoingOrphansDto
{
    public List<OrphanOptionDto> Attached { get; set; } = new();
    public List<OrphanOptionDto> Unattached { get; set; } = new();
}

public class AttachOrphanDto
{
    public Guid OrphanId { get; set; }
}

// ========== Orphans-by-outgoing-letter report (epic 16, UC-COR-19) ==========

public class OutgoingOrphanReportFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? Serial { get; set; }
    public int? Year { get; set; }
    public Guid? CharityId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    /// <summary>The orphan code (كود اليتيم) — mandatory per §21.S.7.</summary>
    public string? ChildCode { get; set; }
}

/// <summary>
/// One row of the §21.S.7 report: an outgoing letter carrying the requested orphan's
/// report, with how many orphans the letter carries in total and the attached flag.
/// </summary>
public class OutgoingOrphanReportRowDto
{
    public Guid OutgoingId { get; set; }
    public int? Serial { get; set; }
    public int? Year { get; set; }
    public DateTime? LetterDate { get; set; }
    public string? CharityName { get; set; }
    public int OrphanCount { get; set; }
    public bool OrphanAttached { get; set; }
}
