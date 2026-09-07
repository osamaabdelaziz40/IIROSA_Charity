namespace IIROSA.Domain.Enums;

/// <summary>
/// Family register discriminator (UC-HOU / chapter 11).
/// Regular = the orphan-sponsorship register (epic 5);
/// Housing = families housed in organisation-owned buildings (epic 6);
/// Refugee = the refugee register (epic 7).
/// Note: NOT the living-condition HousingType lookup — that stays on Family.HousingTypeId.
/// </summary>
public enum FamilyType
{
    Regular = 1,
    Housing = 2,
    Refugee = 3
}
