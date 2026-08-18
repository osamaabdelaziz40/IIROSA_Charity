using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Mark as Uploaded DTO - Used for marking group as uploaded (UC-5.7)
/// </summary>
public class MarkAsUploadedDto
{
    /// <summary>
    /// Payment group ID
    /// </summary>
    [Required(ErrorMessage = "Payment group ID is required")]
    public Guid OrphanPaymentId { get; set; }

    /// <summary>
    /// Mark as uploaded (true) or unmark (false)
    /// </summary>
    [Required(ErrorMessage = "Upload status is required")]
    public bool IsUploaded { get; set; }
}
