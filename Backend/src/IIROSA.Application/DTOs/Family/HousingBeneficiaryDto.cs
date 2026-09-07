namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// UC-HOU-07 (§11.U.7 البحث بالكود): one housing-family beneficiary row — a child (the
/// sponsorship-code carrier) or the guardian — for the code resolve + beneficiary picker on
/// the §11.S.3 reports screen. The resolved <see cref="BeneficiaryId"/> +
/// <see cref="ChildOrParent"/> pair is exactly what 6-6's
/// GET /api/PeriodicOrphanReports/by-orphan/{beneficiaryId}?childOrParent= read consumes.
/// </summary>
public class HousingBeneficiaryDto
{
    /// <summary>
    /// The beneficiary key the reports read consumes — the orphan id for a Child row, the
    /// guardian's provider id for a Parent row.
    /// </summary>
    public Guid BeneficiaryId { get; set; }

    /// <summary>
    /// The §11.U.6 discriminator on the wire — "Child" or "Parent".
    /// </summary>
    public string ChildOrParent { get; set; } = nameof(Domain.Enums.ReportBeneficiaryType.Child);

    /// <summary>
    /// Sponsorship code — children only. The guardian is picked from the list, never
    /// resolved by code (legacy البحث بالكود resolves كود الطالب الابن).
    /// </summary>
    public string? Code { get; set; }

    /// <summary>Beneficiary full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>National id.</summary>
    public string? NationalId { get; set; }

    /// <summary>Age hint from the date of birth (null when unknown).</summary>
    public int? Age { get; set; }
}
