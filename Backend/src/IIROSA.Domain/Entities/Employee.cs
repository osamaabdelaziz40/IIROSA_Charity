using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Employee entity - Inherits from FullAuditedEntity (which inherits from Framework.Core.Data.FullAuditedEntityBase<Guid>)
/// Follows Framework.Core audit trail automatically
/// </summary>
public class Employee : FullAuditedEntity
{
    // Audit fields inherited from FullAuditedEntityBase<Guid>:
    // - CreatedOn, CreatedBy
    // - UpdatedOn, UpdatedBy

    // Employee Identification
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? NationalId { get; set; }

    // Personal Information
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    // Contact Information
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }

    // Employment Details
    public int? DepartmentId { get; set; }
    public string? Position { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal? Salary { get; set; }

    // Account Status
    public bool IsActive { get; set; } = true;

    // Additional Information
    public string? Notes { get; set; }

    // Framework.Identity User Integration
    public Guid? FK_UserId { get; set; }

    // Navigation Properties
    public virtual Department? Department { get; set; }
}
