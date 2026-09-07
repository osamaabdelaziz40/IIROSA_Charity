namespace IIROSA.Application.DTOs.Family;

/// <summary>
/// UC-HOU-04 (§11.S.2 بيانات الأسرة الساكنة): the housing-family aggregate for the
/// view/edit round-trip. Inherits the shared <see cref="FamilyDto"/> (family + guardian +
/// allocation labels + audit stamp) and adds the FULL child detail — FamilyDto.Orphans is the
/// summary <see cref="OrphanListDto"/>, which does not carry the §11.S.2 child fields
/// (education stage, grade, profession, department, faculty, school) the edit form must
/// re-fill without loss.
/// </summary>
public class HousingFamilyDetailDto : FamilyDto
{
    /// <summary>
    /// The family's children (Orphan rows of the Housing family) with every §11.S.2 field.
    /// </summary>
    public List<OrphanDto> Children { get; set; } = new();
}
