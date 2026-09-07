using IIROSA.Domain.Enums;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// One review-queue row (UC-FAM-09) — a flat projection of the §10.S.4 grid columns:
/// الرقم · الجمعيه · كود اليتيم · اسم اليتيم كامل · اسم الام · اسم المعيل الجديد · صله القرابه ·
/// الرقم القومي · تاريخ التعديل · سبب التعديل · موافقه. Old/new guardian snapshots travel with
/// the row so the reviewer never opens the family file.
/// </summary>
public class GuardianChangeRequestListDto
{
    /// <summary>الرقم — the request id</summary>
    public Guid Id { get; set; }

    /// <summary>الجمعيه — the raising charity's Arabic name</summary>
    public string? CharityName { get; set; }

    /// <summary>The family the request concerns (for the approval call in 5-10)</summary>
    public Guid FamilyId { get; set; }

    /// <summary>The family's code</summary>
    public string? FamilyCode { get; set; }

    /// <summary>كود اليتيم (snapshot)</summary>
    public string? OrphanCode { get; set; }

    /// <summary>اسم اليتيم كامل (snapshot)</summary>
    public string? OrphanName { get; set; }

    /// <summary>اسم الام (snapshot)</summary>
    public string? MotherName { get; set; }

    /// <summary>The current guardian's name at raise time (old snapshot)</summary>
    public string? OldGuardianName { get; set; }

    /// <summary>The current guardian's national ID at raise time (old snapshot)</summary>
    public string? OldGuardianNationalId { get; set; }

    /// <summary>The current guardian's relationship to the family at raise time (old snapshot) —
    /// the provider's declared relationship, or Father/Mother for parent-designated families</summary>
    public string? OldGuardianRelationship { get; set; }

    /// <summary>اسم المعيل الجديد</summary>
    public string NewGuardianName { get; set; } = string.Empty;

    /// <summary>الرققم القومي — the proposed guardian's national ID</summary>
    public string NewGuardianNationalId { get; set; } = string.Empty;

    /// <summary>صله القرابه</summary>
    public string Relationship { get; set; } = string.Empty;

    /// <summary>سبب التعديل</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Who raised the request (display name)</summary>
    public string RequestedByName { get; set; } = string.Empty;

    /// <summary>تاريخ التعديل — when the request was raised</summary>
    public DateTime CreatedOn { get; set; }

    /// <summary>Workflow status — renders the موافقه column action when Pending</summary>
    public GuardianChangeRequestStatus Status { get; set; }

    /// <summary>Who decided the request (display name) — null while pending (5-10)</summary>
    public string? DecidedBy { get; set; }

    /// <summary>When the decision was recorded — null while pending (5-10)</summary>
    public DateTime? DecidedOn { get; set; }

    /// <summary>Why the request was refused — shown to the charity on rejection (AC 4)</summary>
    public string? RejectionReason { get; set; }
}

/// <summary>
/// Typed review-queue filter (UC-FAM-09). Status defaults to Pending server-side; a Charity-role
/// caller is forced to its own charity regardless of the charity filter it sends.
/// </summary>
public class GuardianChangeRequestFilterDto
{
    /// <summary>Workflow status filter — null means Pending (the queue's default view)</summary>
    public GuardianChangeRequestStatus? Status { get; set; }

    /// <summary>HQ-only charity filter; ignored (overridden) for Charity-role callers</summary>
    public Guid? CharityId { get; set; }

    /// <summary>1-based page number (default 1)</summary>
    public int? PageNumber { get; set; }

    /// <summary>Page size (default 10, max 100)</summary>
    public int? PageSize { get; set; }
}

/// <summary>
/// Raise a guardian-change request (UC-FAM-09 producer): the proposed new guardian snapshot plus
/// the reason. The old-guardian snapshot is taken server-side from the family file at raise time.
/// </summary>
public class CreateGuardianChangeRequestDto
{
    /// <summary>اسم المعيل الجديد (required)</summary>
    public string NewGuardianName { get; set; } = string.Empty;

    /// <summary>الرقم القومي — the new guardian's national ID (required)</summary>
    public string NewGuardianNationalId { get; set; } = string.Empty;

    /// <summary>صله القرابه (required)</summary>
    public string Relationship { get; set; } = string.Empty;

    /// <summary>سبب التعديل (required)</summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// The head-office decision on a guardian-change request (UC-FAM-10 اعتماد تعديل العائل).
/// Approving applies the proposed guardian to the family file; refusing requires a reason.
/// </summary>
public class ApproveGuardianChangeRequestDto
{
    /// <summary>True = approve and apply; false = reject (RejectionReason becomes mandatory)</summary>
    public bool IsApproved { get; set; }

    /// <summary>Why the request was refused — mandatory when <see cref="IsApproved"/> is false</summary>
    public string? RejectionReason { get; set; }
}
