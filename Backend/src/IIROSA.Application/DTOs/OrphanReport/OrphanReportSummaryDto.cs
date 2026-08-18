namespace IIROSA.Application.DTOs.OrphanReport;

/// <summary>
/// Summary statistics for Orphan Report - UC-6.1
/// </summary>
public class OrphanReportSummaryDto
{
    /// <summary>
    /// Total orphans in report - UC-6.1
    /// </summary>
    public int TotalOrphans { get; set; }

    /// <summary>
    /// Orphans by sponsorship status - UC-6.1
    /// </summary>
    public int SponsoredCount { get; set; }
    public int UnsponsoredCount { get; set; }
    public int PendingCount { get; set; }

    /// <summary>
    /// Orphans by age group - UC-6.1
    /// </summary>
    public int Age0To5Count { get; set; }
    public int Age6To12Count { get; set; }
    public int Age13To18Count { get; set; }
    public int Age19PlusCount { get; set; }

    /// <summary>
    /// Orphans by gender - UC-6.1
    /// </summary>
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }

    /// <summary>
    /// Breakdown by charity (if applicable) - UC-6.1
    /// </summary>
    public List<CharityBreakdownDto>? CharityBreakdown { get; set; }

    /// <summary>
    /// Report period - UC-6.1
    /// </summary>
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

/// <summary>
/// Charity breakdown for summary - UC-6.1
/// </summary>
public class CharityBreakdownDto
{
    public Guid CharityId { get; set; }
    public string CharityName { get; set; } = string.Empty;
    public int OrphanCount { get; set; }
    public int SponsoredCount { get; set; }
    public int UnsponsoredCount { get; set; }
}
