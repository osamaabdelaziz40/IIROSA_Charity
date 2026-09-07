using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Refuse reason catalogue (UC-ORR-08, epic 9) — the drop-down head office picks
/// from when rejecting a periodic report. Inherits the standard lookup shape
/// (Id int, Name/NameAr/NameEn, IsActive, SortOrder, audit fields) from LookupEntity.
/// </summary>
public class RefuseReason : LookupEntity
{
}
