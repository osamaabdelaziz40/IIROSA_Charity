using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Update Project Progress DTO - Used for tracking construction progress (UC-10.4)
/// </summary>
public class UpdateProjectProgressDto
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Project status is required")]
    [StringLength(50, ErrorMessage = "Project status cannot exceed 50 characters")]
    public string ProjectStatus { get; set; } = string.Empty; // Planning, In Progress, Completed, On Hold

    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    public int CompletionPercentage { get; set; }

    [StringLength(100, ErrorMessage = "Current stage cannot exceed 100 characters")]
    public string? CurrentStage { get; set; } // Foundation, Structure, Finishing, Completed

    [StringLength(2000, ErrorMessage = "Progress notes cannot exceed 2000 characters")]
    public string? ProgressNotes { get; set; }
}
