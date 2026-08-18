namespace IIROSA.Application.DTOs.OrphanReport;

/// <summary>
/// Individual Orphan Report Detail - UC-6.1, UC-6.6
/// </summary>
public class OrphanReportDto
{
    // Basic Orphan Information
    public Guid OrphanId { get; set; }
    public string OrphanCode { get; set; } = string.Empty;
    public string OrphanName { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? OrphanType { get; set; }
    public string? SponsorshipStatus { get; set; }

    // Charity Information - UC-6.4
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }

    // Location Information - UC-6.5
    public int? RegionId { get; set; }
    public string? RegionName { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }

    // Sponsor Information
    public Guid? SponsorId { get; set; }
    public string? SponsorName { get; set; }
    public decimal? MonthlyAmount { get; set; }

    #region Family Details (UC-6.6) - Included if IncludeFamilyDetails = true

    public string? FamilyAddress { get; set; }
    public string? FatherName { get; set; }
    public string? MotherName { get; set; }
    public string? ProviderName { get; set; }
    public string? FamilyPhone { get; set; }

    #endregion

    #region Contact Information (UC-6.1) - Included if IncludeContactInformation = true

    public string? OrphanPhone { get; set; }
    public string? OrphanEmail { get; set; }

    #endregion

    #region Education Details (UC-6.1) - Included if IncludeEducationDetails = true

    public int? EducationLevelId { get; set; }
    public string? EducationLevelName { get; set; }
    public string? SchoolName { get; set; }
    public string? GradeClass { get; set; }
    public string? AcademicPerformance { get; set; }

    #endregion

    #region Health Details (UC-6.1) - Included if IncludeHealthDetails = true

    public int? HealthStatusId { get; set; }
    public string? HealthStatusName { get; set; }
    public string? Disabilities { get; set; }
    public string? ChronicDiseases { get; set; }

    #endregion

    #region Photo

    public Guid? PhotoAttachmentId { get; set; }
    public string? PhotoUrl { get; set; }

    #endregion
}
