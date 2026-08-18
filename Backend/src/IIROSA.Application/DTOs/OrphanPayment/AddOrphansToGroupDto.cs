using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Add Orphans to Payment Group DTO - Used for adding orphans to a payment group (UC-5.3)
/// </summary>
public class AddOrphansToGroupDto
{
    /// <summary>
    /// Payment group ID
    /// </summary>
    [Required(ErrorMessage = "Payment group ID is required")]
    public Guid OrphanPaymentId { get; set; }

    /// <summary>
    /// List of orphan IDs to add to the group
    /// </summary>
    [Required(ErrorMessage = "At least one orphan must be selected")]
    [MinLength(1, ErrorMessage = "At least one orphan must be selected")]
    public List<Guid> OrphanIds { get; set; } = new();
}
