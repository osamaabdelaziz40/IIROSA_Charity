using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity Location DTO - Used for assigning charity to center (UC-3.12)
/// </summary>
public class CharityLocationDto
{
    [Required]
    public Guid CharityId { get; set; }

    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
}
