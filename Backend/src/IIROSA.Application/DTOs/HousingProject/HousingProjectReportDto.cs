namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Housing Project Report DTO - Used for generating housing reports (UC-10.9)
/// </summary>
public class HousingProjectReportDto
{
    // Report Parameters
    public DateTime ReportStartDate { get; set; }
    public DateTime ReportEndDate { get; set; }
    public string? GroupBy { get; set; }
    public DateTime GeneratedOn { get; set; }

    // Project Summary
    public int TotalProjects { get; set; }
    public Dictionary<string, int> ProjectsByStatus { get; set; } = new();
    public Dictionary<string, int> ProjectsByType { get; set; } = new();
    public Dictionary<string, int> ProjectsByRegion { get; set; } = new();
    public Dictionary<string, int> ProjectsByCharity { get; set; } = new();

    // Financial Summary
    public decimal TotalBudget { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal AverageCostPerProject { get; set; }
    public Dictionary<string, decimal> BudgetByStatus { get; set; } = new();

    // Progress Summary
    public double AverageCompletionPercentage { get; set; }
    public int ProjectsOnTrack { get; set; }
    public int DelayedProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int ActiveProjects { get; set; }

    // Beneficiary Summary
    public int FamiliesHoused { get; set; }
    public int IndividualsBenefited { get; set; }
    public int TotalUnitsBuilt { get; set; }
    public decimal TotalAreaBuilt { get; set; } // sq meters

    // Detailed Project List
    public List<HousingProjectReportDetailDto> ProjectDetails { get; set; } = new();
}

/// <summary>
/// Housing Project Report Detail DTO - Individual project in report
/// </summary>
public class HousingProjectReportDetailDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;
    public string? RegionName { get; set; }
    public string? CharityName { get; set; }
    public string? FamilyCode { get; set; }
    public string ProjectStatus { get; set; } = string.Empty;
    public int CompletionPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal? FinalCost { get; set; }
    public string BudgetCurrency { get; set; } = "EGP";
    public int? NumberOfUnits { get; set; }
    public decimal? TotalArea { get; set; }
    public bool IsDelayed { get; set; }
}
