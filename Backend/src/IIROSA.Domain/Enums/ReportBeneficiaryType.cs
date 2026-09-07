namespace IIROSA.Domain.Enums;

/// <summary>
/// UC-HOU-06/08 (§11.S.3 التقارير الدورية للأسر الساكنة) — the ChildOrParent discriminator:
/// which kind of housing beneficiary a periodic report is about. Existing (pre-housing) rows
/// are child reports by definition — the column defaults to Child and the 6-6 migration
/// backfills accordingly.
/// </summary>
public enum ReportBeneficiaryType
{
    /// <summary>The report is about one of the family's children (an Orphan row).</summary>
    Child = 1,

    /// <summary>The report is about the family's guardian (the Provider of the housing family).</summary>
    Parent = 2
}
