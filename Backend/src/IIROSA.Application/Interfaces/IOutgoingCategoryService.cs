using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Outgoing Category Service Interface (UC-COR-17 · تصنيف الصادر)
/// Rides the generic lookup machinery for the lookup-management screen (UC-14.5)
/// and the outgoing letter form's category drop-down.
/// </summary>
public interface IOutgoingCategoryService : ILookupService<OutgoingCategory, LookupDto, CreateLookupDto, UpdateLookupDto>
{
}
