namespace IIROSA.Application.DTOs.HousingProject;

/// <summary>
/// Housing Project List DTO - Used for grid/list views (UC-10.6)
/// </summary>
public class HousingProjectListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectType { get; set; } = string.Empty;
    public string? RegionName { get; set; }
    public string? CenterName { get; set; }
    public string? Address { get; set; }
    public string? CharityName { get; set; }
    public string? FamilyCode { get; set; }
    public string ProjectStatus { get; set; } = "Planning";
    public int CompletionPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public decimal TotalBudget { get; set; }
    public string BudgetCurrency { get; set; } = "EGP";
    public bool IsCompleted { get; set; }
    public bool IsDelayed { get; set; }
}
