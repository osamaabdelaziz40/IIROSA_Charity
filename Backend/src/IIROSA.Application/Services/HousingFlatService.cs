using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Housing Flat Service (UC-HOU-05) — rides the generic lookup machinery with rich DTOs
/// (Number / SizeInMtr / Description / BuildingId) and adds the per-building cascade the
/// §11.S.2 form needs (رقم الشقه repopulates on رقم العماره change)
/// </summary>
public class HousingFlatService : LookupServiceBase<HousingFlat, HousingFlatDto, CreateHousingFlatDto, UpdateHousingFlatDto>, IHousingFlatService
{
    public HousingFlatService(
        ILookupRepository<HousingFlat> repository,
        IMapper mapper,
        ILogger<HousingFlatService> logger) : base(repository, mapper, logger)
    {
    }

    /// <inheritdoc />
    public async Task<List<HousingFlatDto>> GetFlatsByBuildingAsync(int buildingId, bool includeInactive = false)
    {
        // Review 2026-08-24: the predicate runs server-side (Where on the IQueryable) —
        // the old GetAllAsync() materialized the whole flat table to filter in memory.
        var query = _repository.AsQueryable()
            .Where(f => f.BuildingId == buildingId);

        if (!includeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        var flats = await query
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Id)
            .ToListAsync();

        return _mapper.Map<List<HousingFlatDto>>(flats);
    }
}
