using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Enums;

namespace IIROSA.Domain.Entities;

/// <summary>
/// GuardianChangeRequest entity — a charity's request to change a family's guardian (العائل),
/// decided by the head office (UC-FAM-09 طلبات تعديل العائل / UC-FAM-10 الموافقة).
///
/// The request carries FULL old/new guardian snapshots (name, national ID, relationship) taken at
/// raise time, so the reviewer compares what was there against what is proposed without opening
/// the family file — never join live data for the old-guardian columns. One pending request per
/// family at a time (legacy guard «تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب»).
/// Decided fields (Status/DecidedBy/DecidedOn/RejectionReason) are written by the approval story
/// (5-10); they are declared here so that story needs no second migration.
/// </summary>
public class GuardianChangeRequest : FullAuditedEntity
{
    // ========== Family & Charity (الأسرة / الجمعية) ==========

    /// <summary>
    /// Foreign key to the family whose guardian change is requested (required)
    /// </summary>
    public Guid FamilyId { get; set; }

    /// <summary>
    /// The charity that raised the request — stamped from the family, never from the payload
    /// </summary>
    public Guid CharityId { get; set; }

    // ========== Orphan context (سياق اليتيم — optional snapshot columns) ==========

    /// <summary>
    /// كود اليتيم the change concerns (snapshot; nullable when family-wide)
    /// </summary>
    public string? OrphanCode { get; set; }

    /// <summary>
    /// اسم اليتيم كامل (snapshot)
    /// </summary>
    public string? OrphanName { get; set; }

    /// <summary>
    /// اسم الام (snapshot)
    /// </summary>
    public string? MotherName { get; set; }

    // ========== Old guardian snapshot (العائل الحالي) ==========

    /// <summary>
    /// Current guardian's full name at raise time
    /// </summary>
    public string? OldGuardianName { get; set; }

    /// <summary>
    /// Current guardian's national ID at raise time
    /// </summary>
    public string? OldGuardianNationalId { get; set; }

    /// <summary>
    /// Current guardian's relationship to the family at raise time — from the provider row, or
    /// the Father/Mother designation itself for parent-guardian families (DoD §10.U.09 names it;
    /// landed 2026-08-24 with the one-pending index)
    /// </summary>
    public string? OldGuardianRelationship { get; set; }

    // ========== New guardian proposal (العائل الجديد) ==========

    /// <summary>
    /// Proposed guardian's full name (required)
    /// </summary>
    public string NewGuardianName { get; set; } = string.Empty;

    /// <summary>
    /// Proposed guardian's national ID (required)
    /// </summary>
    public string NewGuardianNationalId { get; set; } = string.Empty;

    /// <summary>
    /// صله القرابه — the proposed guardian's relationship to the family
    /// </summary>
    public string Relationship { get; set; } = string.Empty;

    // ========== Reason & workflow (السبب / الحالة) ==========

    /// <summary>
    /// سبب التعديل — why the guardian must change (required)
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Who raised the request (display name for the queue)
    /// </summary>
    public string RequestedByName { get; set; } = string.Empty;

    /// <summary>
    /// Workflow status — Pending until the head office decides (5-10 writes Approved/Rejected)
    /// </summary>
    public GuardianChangeRequestStatus Status { get; set; } = GuardianChangeRequestStatus.Pending;

    /// <summary>
    /// Who decided the request (written by 5-10)
    /// </summary>
    public string? DecidedBy { get; set; }

    /// <summary>
    /// When the decision was made (written by 5-10)
    /// </summary>
    public DateTime? DecidedOn { get; set; }

    /// <summary>
    /// Why the request was rejected (written by 5-10)
    /// </summary>
    public string? RejectionReason { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation to the family the request concerns
    /// </summary>
    public virtual Family? Family { get; set; }

    /// <summary>
    /// Navigation to the raising charity
    /// </summary>
    public virtual Charity? Charity { get; set; }
}
