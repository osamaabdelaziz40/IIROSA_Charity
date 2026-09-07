using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Housing Flat Service (UC-HOU-05) — rides the generic lookup machinery and adds the
/// per-building cascade the §11.S.2 form needs (رقم الشقه repopulates on رقم العماره change)
/// </summary>
public class HousingFlatService : LookupServiceBase<HousingFlat, LookupDto, CreateLookupDto, UpdateLookupDto>, IHousingFlatService
{
    public HousingFlatService(
        ILookupRepository<HousingFlat> repository,
        IMapper mapper,
        ILogger<HousingFlatService> logger) : base(repository, mapper, logger)
    {
    }

    /// <inheritdoc />
    public async Task<List<LookupDto>> GetFlatsByBuildingAsync(int buildingId)
    {
        // Review 2026-08-24: the predicate runs server-side (Where on the IQueryable) —
        // the old GetAllAsync() materialized the whole flat table to filter in memory.
        var flats = await _repository.AsQueryable()
            .Where(f => f.BuildingId == buildingId && f.IsActive)
            .OrderBy(f => f.SortOrder)
            .ThenBy(f => f.Id)
            .ToListAsync();

        return _mapper.Map<List<LookupDto>>(flats);
    }
}
