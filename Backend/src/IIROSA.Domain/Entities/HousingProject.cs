using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Housing Project entity - Implements all use cases UC-10.1 through UC-10.10
/// Inherits from FullAuditedEntityBase<Guid> with all audit fields
/// </summary>
public class HousingProject : FullAuditedEntity
{
    // Basic Information (UC-10.1)
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectType { get; set; } = string.Empty; // New Construction, Renovation, Repair, Expansion
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    // Location (UC-10.1)
    public int? CountryId { get; set; }
    public int? RegionId { get; set; }
    public int? CenterId { get; set; }
    public string? Address { get; set; }
    public string? Village { get; set; }
    public string? GPSCoordinates { get; set; }

    // Specifications (UC-10.1)
    public string HousingType { get; set; } = string.Empty; // Apartment, Villa, House, Room
    public int? NumberOfUnits { get; set; }
    public decimal? AreaPerUnit { get; set; } // sq meters
    public decimal? TotalArea { get; set; } // sq meters

    // Financial Information (UC-10.2)
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP"; // EGP, SAR
    public string? DonorName { get; set; }
    public decimal? FinalCost { get; set; }

    // Beneficiary Assignment (UC-10.3, UC-10.10)
    public Guid? CharityId { get; set; }
    public Guid? FamilyId { get; set; }

    // Status and Progress (UC-10.4, UC-10.5, UC-10.7)
    public string ProjectStatus { get; set; } = "Planning"; // Planning, In Progress, Completed, On Hold
    public int CompletionPercentage { get; set; } = 0;
    public string? CurrentStage { get; set; } // Foundation, Structure, Finishing, Completed
    public string? ProgressNotes { get; set; }

    // Completion Information (UC-10.5)
    public string? CompletionNotes { get; set; }
    public string? HandoverDocumentId { get; set; } // Reference to Framework.Core Attachment entity

    // Calculated fields (not stored in database)
    [System.Text.Json.Serialization.JsonIgnore]
    public decimal BudgetRemaining => FinalCost.HasValue ? TotalBudget - FinalCost.Value : TotalBudget;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsCompleted => ProjectStatus == "Completed";

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsDelayed => ExpectedEndDate.HasValue && ActualEndDate.HasValue && ActualEndDate > ExpectedEndDate;

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsOnTrack => !IsDelayed && CompletionPercentage > 0;

    // Navigation Properties
    public virtual Country? Country { get; set; }
    public virtual Region? Region { get; set; }
    public virtual Center? Center { get; set; }
    public virtual Charity? Charity { get; set; }
    public virtual Family? Family { get; set; }
}
