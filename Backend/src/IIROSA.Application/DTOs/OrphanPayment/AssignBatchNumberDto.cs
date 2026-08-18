using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanPayment;

/// <summary>
/// Assign Batch Number DTO - Used for assigning batch number (UC-5.11)
/// </summary>
public class AssignBatchNumberDto
{
    /// <summary>
    /// Payment group ID
    /// </summary>
    [Required(ErrorMessage = "Payment group ID is required")]
    public Guid OrphanPaymentId { get; set; }

    /// <summary>
    /// Batch number (leave empty to auto-generate)
    /// </summary>
    [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters")]
    public string? BatchNo { get; set; }

    /// <summary>
    /// Auto-generate next batch number
    /// </summary>
    public bool AutoGenerate { get; set; } = false;
}
