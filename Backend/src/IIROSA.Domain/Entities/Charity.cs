using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Charity entity - Inherits from FullAuditedEntityBase<Guid>
/// Implements all use cases UC-3.1 through UC-3.14
/// All audit fields (CreatedOn, UpdatedOn, CreatedBy, UpdatedBy, DeletedOn, DeletedBy, IsDeleted) are inherited
/// </summary>
public class Charity : FullAuditedEntity
{
    // Basic Information
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NGOType { get; set; }

    // Contact Information
    public string Address { get; set; } = string.Empty;
    public string? StreetName { get; set; }
    public string? Village { get; set; }
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
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? NgoMapLocation { get; set; }

    // Banking
    public int? BankId { get; set; }
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
    public bool ReceivingDonations { get; set; } = true;
    public string? Notes { get; set; }

    // User Account
    public string? UserId { get; set; }

    // Rights Management (UC-3.6, UC-3.7, UC-3.8)
    public bool IsAddEnabled { get; set; } = true;
    public bool IsUpdateEnabled { get; set; } = true;
    public bool IsLocked { get; set; } = false;

    // Status (UC-3.3, UC-3.4)
    public bool IsActive { get; set; } = true;

    // Legacy Fields (for backward compatibility)
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? LicenseNumber { get; set; }
    public DateTime? LicenseExpiryDate { get; set; }
    public DateTime? EstablishmentDate { get; set; }
    public string? Website { get; set; }

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual Region? Region { get; set; }
    public virtual Center? Center { get; set; }
    public virtual Bank? Bank { get; set; }
    public virtual City? City { get; set; }

    public virtual ICollection<Family> Families { get; set; } = new List<Family>();
    public virtual ICollection<Orphan> Orphans { get; set; } = new List<Orphan>();
    public virtual ICollection<Sponsor> Sponsors { get; set; } = new List<Sponsor>();
}
