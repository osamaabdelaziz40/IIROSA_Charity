namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Transfer a family to another charity (UC-FAM-06 نقل الأسرة لجمعية أخرى).
/// HQ-only operation; the receiving charity is validated server-side before anything is written.
/// </summary>
public class TransferFamilyDto
{
    /// <summary>
    /// The receiving charity (اختر الجمعية)
    /// </summary>
    public Guid NewCharityId { get; set; }

    /// <summary>
    /// سبب النقل (optional, free text)
    /// </summary>
    public string? Reason { get; set; }
}
