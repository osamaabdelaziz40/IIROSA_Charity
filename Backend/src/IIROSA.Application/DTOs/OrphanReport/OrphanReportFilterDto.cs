using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.OrphanReport;

/// <summary>
/// Filter DTO for Orphan Reports - Implements UC-6.1, UC-6.2, UC-6.3, UC-6.4, UC-6.5
/// </summary>
public class OrphanReportFilterDto
{
    #region Report Period (UC-6.2)

    /// <summary>
    /// From Date (Required) - UC-6.1, UC-6.2
    /// </summary>
    [Required(ErrorMessage = "From date is required")]
    public DateTime FromDate { get; set; }

    /// <summary>
    /// To Date (Required) - UC-6.1, UC-6.2
    /// </summary>
    [Required(ErrorMessage = "To date is required")]
    public DateTime ToDate { get; set; }

    #endregion

    #region Filters (UC-6.1)

    /// <summary>
    /// Filter by Charity (Admin/Super Admin only) - UC-6.4
    /// </summary>
    public Guid? CharityId { get; set; }

    /// <summary>
    /// رقم التقرير — §14.U.11 extract criterion (contains match on the periodic report number)
    /// </summary>
    public string? ReportNo { get; set; }

    /// <summary>
    /// §14.U.15 أرقام التقارير المضافة — fill the statistics response's ReportNumbers
    /// branch (the numbers-in-period extract). Requires the date window; the grouped
    /// branch (9-10) is untouched.
    /// </summary>
    public bool IncludeReportNumbers { get; set; }

    /// <summary>
    /// Filter by Region - UC-6.5
    /// </summary>
    public int? RegionId { get; set; }

    /// <summary>
    /// Filter by Center - UC-6.1
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Filter by Sponsorship Status - UC-6.3
    /// Values: Sponsored, Unsponsored, Pending, All
    /// </summary>
    public string? SponsorshipStatus { get; set; }

    /// <summary>
    /// Age Range From - UC-6.1
    /// </summary>
    public int? AgeFrom { get; set; }

    /// <summary>
    /// Age Range To - UC-6.1
    /// </summary>
    public int? AgeTo { get; set; }

    /// <summary>
    /// Filter by Gender - UC-6.1
    /// Values: Male, Female, All
    /// </summary>
    public string? Gender { get; set; }

    #endregion

    #region Report Options (UC-6.1, UC-6.6)

    /// <summary>
    /// Include Family Details - UC-6.6
    /// </summary>
    public bool IncludeFamilyDetails { get; set; } = false;

    /// <summary>
    /// Include Contact Information - UC-6.1
    /// </summary>
    public bool IncludeContactInformation { get; set; } = false;

    /// <summary>
    /// Include Education Details - UC-6.1
    /// </summary>
    public bool IncludeEducationDetails { get; set; } = false;

    /// <summary>
    /// Include Health Details - UC-6.1
    /// </summary>
    public bool IncludeHealthDetails { get; set; } = false;

    /// <summary>
    /// Group By Charity (Admin/Super Admin only) - UC-6.1
    /// </summary>
    public bool GroupByCharity { get; set; } = false;

    #endregion
}
