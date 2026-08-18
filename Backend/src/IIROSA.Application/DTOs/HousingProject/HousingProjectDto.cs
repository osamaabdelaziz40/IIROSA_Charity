namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Housing Project DTO - Full details for single project view
/// </summary>
public class HousingProjectDto
{
    public Guid Id { get; set; }

    // Basic Information
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProjectType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }

    // Location
    public int? CountryId { get; set; }
    public string? CountryName { get; set; }
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }
    public string? Address { get; set; }
    public string? Village { get; set; }
    public string? GPSCoordinates { get; set; }

    // Specifications
    public string HousingType { get; set; } = string.Empty;
    public int? NumberOfUnits { get; set; }
    public decimal? AreaPerUnit { get; set; }
    public decimal? TotalArea { get; set; }

    // Financial Information
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP";
    public string? DonorName { get; set; }
    public decimal? FinalCost { get; set; }
    public decimal BudgetRemaining { get; set; }

    // Beneficiary Assignment
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public Guid? FamilyId { get; set; }
    public string? FamilyCode { get; set; }
    public string? FamilyAddress { get; set; }

    // Status and Progress
    public string ProjectStatus { get; set; } = "Planning";
    public int CompletionPercentage { get; set; }
    public string? CurrentStage { get; set; }
    public string? ProgressNotes { get; set; }

    // Completion Information
    public string? CompletionNotes { get; set; }
    public string? HandoverDocumentId { get; set; }

    // Calculated properties
    public bool IsCompleted { get; set; }
    public bool IsDelayed { get; set; }
    public bool IsOnTrack { get; set; }

    // Audit fields
    public DateTime CreatedOn { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public string? UpdatedBy { get; set; }
}
