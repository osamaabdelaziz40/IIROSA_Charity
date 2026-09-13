using System.ComponentModel.DataAnnotations;

namespace IIROSA.Application.DTOs.SeasonalAid;

/// <summary>
/// Update Seasonal Aid Beneficiaries DTO - full-sync payload for UC-PRJ-07.
/// <see cref="FamilyIds"/> carries the desired final set of families registered for the
/// campaign: families present only in the stored set are removed, families present only in
/// the payload are added, and families in both keep their registration as-is. An empty list
/// is a valid deselect-all request.
/// </summary>
public class UpdateSeasonalAidBeneficiariesDto
{
    /// <summary>The desired final set of family ids (CampaignId comes from the route).</summary>
    public List<Guid> FamilyIds { get; set; } = new();

    [Range(0.01, double.MaxValue, ErrorMessage = "Allocation amount must be greater than zero")]
    public decimal? AllocationAmount { get; set; }

    [StringLength(3, ErrorMessage = "Currency code cannot exceed 3 characters")]
    public string? Currency { get; set; }

    [StringLength(500, ErrorMessage = "Registration notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Outcome of a beneficiaries full-sync, echoed back to the selection screen so it can
/// refresh its counters (registered vs quota) without a follow-up read.
/// </summary>
public class UpdateBeneficiariesResultDto
{
    public int AddedCount { get; set; }
    public int RemovedCount { get; set; }
    public int TotalRegistered { get; set; }
    public int? MaximumFamilies { get; set; }
}

/// <summary>
/// Move one registration between the selection screen's two lists: main families (true)
/// or the pending list (false) — UC-PRJ-06.
/// </summary>
public class SetBeneficiaryMainStatusDto
{
    public bool IsMain { get; set; }
}
