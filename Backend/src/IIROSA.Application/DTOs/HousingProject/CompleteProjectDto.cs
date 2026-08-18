using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Complete Project DTO - Used for recording project completion (UC-10.5)
/// </summary>
public class CompleteProjectDto
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Actual end date is required")]
    public DateTime ActualEndDate { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Final cost must be greater than zero")]
    public decimal FinalCost { get; set; }

    [StringLength(2000, ErrorMessage = "Completion notes cannot exceed 2000 characters")]
    public string? CompletionNotes { get; set; }

    public string? HandoverDocumentId { get; set; }
}
