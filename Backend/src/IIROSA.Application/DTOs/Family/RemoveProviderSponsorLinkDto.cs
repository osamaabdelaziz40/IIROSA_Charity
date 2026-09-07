namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// UC-FAM-13 (§10.U.13 حذف كفالة العائل) — removes a guardian's sponsorship link.
/// The التعليق (comment) from the «هل انت متاكد من حذف ؟» modal; recorded on the
/// provider row's notes in the member-control stamp format before the soft delete.
/// </summary>
public class RemoveProviderSponsorLinkDto
{
    /// <summary>
    /// Optional justification for the removal (التعليق), max 500 characters.
    /// </summary>
    public string? Comment { get; set; }
}
