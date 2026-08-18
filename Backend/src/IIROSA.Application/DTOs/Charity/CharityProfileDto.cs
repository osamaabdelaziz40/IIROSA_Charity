namespace IIROSA.Application.DTOs.Charity;

/// <summary>
/// Charity Profile DTO - Used for viewing detailed charity profile (UC-3.11)
/// Charity users see only their own profile, Admin/Super Admin see all
/// </summary>
public class CharityProfileDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NGOType { get; set; }

    // Basic Information
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
    public string? CountryName { get; set; }
    public string? RegionName { get; set; }
    public string? CenterName { get; set; }
    public string? NgoMapLocation { get; set; }

    // Banking
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? IBAN { get; set; }

    // Management Contacts
    public string? BossName { get; set; }
    public string? BossJobName { get; set; }
    public string? BossPhone1 { get; set; }
    public string? BossPhone2 { get; set; }

    public string? ResponsibleJobName { get; set; }
    public string? ResponsiblePhone1 { get; set; }
    public string? ResponsiblePhone2 { get; set; }

    // Settings & Rights
    public Guid? IconId { get; set; }
    public bool ReceivingDonations { get; set; }
    public bool IsAddEnabled { get; set; }
    public bool IsUpdateEnabled { get; set; }
    public bool IsLocked { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    // User Account
    public string? UserId { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }  // Only returned when user account is created
    public DateTime? LastLoginDate { get; set; }

    // Statistics
    public int FamilyCount { get; set; }
    public int OrphanCount { get; set; }
    public int SponsorCount { get; set; }
}
