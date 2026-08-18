namespace IIROSA.Application.DTOs.OrphanReport;

/// <summary>
/// Period comparison for Orphan Reports - UC-6.10
/// </summary>
public class OrphanReportComparisonDto
{
    /// <summary>
    /// First report period
    /// </summary>
    public OrphanReportSummaryDto Report1 { get; set; } = new();

    /// <summary>
    /// Second report period
    /// </summary>
    public OrphanReportSummaryDto Report2 { get; set; } = new();

    /// <summary>
    /// Comparison metrics - UC-6.10
    /// </summary>
    public OrphanReportComparisonMetricsDto Comparison { get; set; } = new();
}

/// <summary>
/// Comparison metrics - UC-6.10
/// </summary>
public class OrphanReportComparisonMetricsDto
{
    /// <summary>
    /// Total orphan count change - UC-6.10
    /// </summary>
    public int TotalCountChange { get; set; }
    public decimal TotalCountPercentChange { get; set; }

    /// <summary>
    /// Sponsored vs Unsponsored changes - UC-6.10
    /// </summary>
    public int SponsoredCountChange { get; set; }
    public int UnsponsoredCountChange { get; set; }

    /// <summary>
    /// Age distribution changes - UC-6.10
    /// </summary>
    public int Age0To5CountChange { get; set; }
    public int Age6To12CountChange { get; set; }
    public int Age13To18CountChange { get; set; }
    public int Age19PlusCountChange { get; set; }

    /// <summary>
    /// Gender distribution changes - UC-6.10
    /// </summary>
    public int MaleCountChange { get; set; }
    public int FemaleCountChange { get; set; }

    /// <summary>
    /// Charity-by-charity breakdown - UC-6.10
    /// </summary>
    public List<CharityComparisonDto>? CharityComparison { get; set; }
}

/// <summary>
/// Charity comparison for period comparison - UC-6.10
/// </summary>
public class CharityComparisonDto
{
    public Guid CharityId { get; set; }
    public string CharityName { get; set; } = string.Empty;
    public int Report1Count { get; set; }
    public int Report2Count { get; set; }
    public int CountChange { get; set; }
    public decimal PercentChange { get; set; }
}
