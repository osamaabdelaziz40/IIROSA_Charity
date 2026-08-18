using Framework.Core.SharedServices.Dto;

namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity DTO - Full charity details
/// Used for displaying complete charity information
/// </summary>
public class CharityDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NGOType { get; set; }

    // Contact Information
    public string Address { get; set; } = string.Empty;
    public string? StreetName { get; set; }
    public string? Village { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? MailBox { get; set; }

    // Phone/Contact
    public string Phone { get; set; } = string.Empty;
    public string? Phone2 { get; set; }
    public string? HomePhone { get; set; }
    public string? Fax { get; set; }
    public string Email { get; set; } = string.Empty;

    // Location
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public string? NgoMapLocation { get; set; }

    // Banking
    public int? BankId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? IBAN { get; set; }

    // Management - Boss
    public string? BossName { get; set; }
    public string? BossJobName { get; set; }
    public string? BossPhone1 { get; set; }
    public string? BossPhone2 { get; set; }

    // Management - Responsible
    public string? ResponsibleJobName { get; set; }
    public string? ResponsiblePhone1 { get; set; }
    public string? ResponsiblePhone2 { get; set; }

    // Settings
    public Guid? IconId { get; set; }
    public List<AttachmentDto>? Icon_Attach { get; set; }
    public bool ReceivingDonations { get; set; }
    public string? Notes { get; set; }

    // User Account
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }  // Only returned when user account is created
    public DateTime? LastLogin { get; set; }

    // Rights Management
    public bool IsAddEnabled { get; set; }
    public bool IsUpdateEnabled { get; set; }
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }

    // Audit
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }

    // Statistics
    public int FamilyCount { get; set; }
    public int OrphanCount { get; set; }
    public int SponsorCount { get; set; }
}
