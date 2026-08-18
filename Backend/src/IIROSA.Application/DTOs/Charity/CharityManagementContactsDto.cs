using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity Management Contacts DTO - Used for updating management contacts (UC-3.13)
/// </summary>
public class CharityManagementContactsDto
{
    [Required]
    public Guid CharityId { get; set; }

    // Boss Information
    [StringLength(100, ErrorMessage = "Boss name cannot exceed 100 characters")]
    public string? BossName { get; set; }

    [StringLength(100, ErrorMessage = "Boss job name cannot exceed 100 characters")]
    public string? BossJobName { get; set; }

    [StringLength(20, ErrorMessage = "Boss phone1 cannot exceed 20 characters")]
    public string? BossPhone1 { get; set; }

    [StringLength(20, ErrorMessage = "Boss phone2 cannot exceed 20 characters")]
    public string? BossPhone2 { get; set; }

    // Responsible Information
    [StringLength(100, ErrorMessage = "Responsible job name cannot exceed 100 characters")]
    public string? ResponsibleJobName { get; set; }

    [StringLength(20, ErrorMessage = "Responsible phone1 cannot exceed 20 characters")]
    public string? ResponsiblePhone1 { get; set; }

    [StringLength(20, ErrorMessage = "Responsible phone2 cannot exceed 20 characters")]
    public string? ResponsiblePhone2 { get; set; }
}
