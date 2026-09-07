namespace IIROSA.Domain.Enums;

/// <summary>
/// Guardian-change request workflow status (UC-FAM-09 طلبات تعديل العائل).
/// A Domain enum — not a lookup table: one workflow, closed set, no seeding needed
/// (lighter than the SupportTicket lookup pattern; pick one and stay consistent).
/// </summary>
public enum GuardianChangeRequestStatus
{
    /// <summary>Waiting for the head office (General Director) decision — the review-queue default.</summary>
    Pending = 1,

    /// <summary>5-10 approved the change and applied the new guardian snapshot.</summary>
    Approved = 2,

    /// <summary>5-10 rejected the change; RejectionReason carries why.</summary>
    Rejected = 3
}
