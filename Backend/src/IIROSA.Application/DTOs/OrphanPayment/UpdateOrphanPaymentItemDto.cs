namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// §15.1 row-action envelope — ONE endpoint (POST /api/OrphanPayments/orphan-items) for the
/// whole flag family. Property names verbatim from §15.1/§15.U.12 so 10-10..10-12 add no fields.
/// Action map: 0 = stop/resume (Flag carries the value) · 1 = mark printed · 2 = confirm receipt
/// · 3 = record cheque (ChiqueNum + ChiqueDate→Printdate + BenificiaryName) · 4 = clear flags
/// (10-13). Stories 10-10..10-13 implement actions 1..4; this DTO is frozen.
/// </summary>
public class UpdateOrphanPaymentItemDto
{
    /// <summary>The OrphanPaymentItem row the action targets — not the orphan id.</summary>
    public Guid OrphanPaymentItemId { get; set; }

    /// <summary>§15.1 action discriminator, 0..4.</summary>
    public int Action { get; set; }

    /// <summary>Action 0 value: true = stop the row, false = resume it.</summary>
    public bool? Flag { get; set; }

    /// <summary>Cheque number (action 3).</summary>
    public string? ChiqueNum { get; set; }

    /// <summary>Cheque/print date (action 3) — maps to the row's Printdate column.</summary>
    public DateTime? ChiqueDate { get; set; }

    /// <summary>Collector name at the charity (action 3).</summary>
    public string? BenificiaryName { get; set; }
}
