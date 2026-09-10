using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// Family phone — read model (UC-ORP-10 contact numbers, one marked default).
/// </summary>
public class FamilyPhoneDto
{
    public Guid Id { get; set; }
    public Guid FamilyId { get; set; }

    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string Number { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}

/// <summary>
/// Family phone — write model (create/update payload rows).
/// </summary>
public class CreateFamilyPhoneDto
{
    [Required(ErrorMessage = "Phone number is required")]
    [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
    public string Number { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
}
