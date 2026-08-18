using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Office Project Type Service Interface
/// Extends ILookupService for common lookup operations
/// Implements UC-7.1: Office Project Type management
/// </summary>
public interface IOfficeProjectTypeService : ILookupService<OfficeProjectType, LookupDto, CreateLookupDto, UpdateLookupDto>
{
}
