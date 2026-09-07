using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Outgoing Category Service (UC-COR-17) — plain lookup, base DTOs suffice:
/// NameAr/NameEn/Description/IsActive/SortOrder with no extra columns.
/// </summary>
public class OutgoingCategoryService : LookupServiceBase<OutgoingCategory, LookupDto, CreateLookupDto, UpdateLookupDto>, IOutgoingCategoryService
{
    public OutgoingCategoryService(
        ILookupRepository<OutgoingCategory> repository,
        IMapper mapper,
        ILogger<OutgoingCategoryService> logger) : base(repository, mapper, logger)
    {
    }
}
