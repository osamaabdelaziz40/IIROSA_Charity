using IIROSA.Application.DTOs.Charity;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Charity Repository Implementation
/// Provides data access operations for Charity entity
/// </summary>
public class CharityRepository : Repository<Charity>, ICharityRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<Charity> _dbSet;

    public CharityRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<Charity>();
    }

    #region Basic CRUD

    public async Task<Charity?> GetByCodeAsync(string code)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<Charity?> GetByUserIdAsync(string userId)
    {
        return await IncludeNavigationProperties()
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    #endregion

    #region Search and Filter

    public async Task<IEnumerable<Charity>> SearchAsync(string searchTerm)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted &&
                        (c.Name.Contains(searchTerm) ||
                         c.Code.Contains(searchTerm) ||
                         c.Email.Contains(searchTerm)))
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Charity>> GetActiveCharitiesAsync()
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Charity>> GetByCountryAsync(int countryId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.CountryId == countryId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Charity>> GetByRegionAsync(int regionId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.RegionId == regionId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Charity>> GetByCenterAsync(int centerId)
    {
        return await IncludeNavigationProperties()
            .Where(c => !c.IsDeleted && c.CenterId == centerId)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Charity> Items, int TotalCount)> GetFilteredPaginatedAsync(
        string? searchTerm = null,
        int? countryId = null,
        int? regionId = null,
        int? centerId = null,
        bool? isActive = null,
        bool? isLocked = null,
        bool? isAddEnabled = null,
        bool? isUpdateEnabled = null,
        int pageNumber = 1,
        int pageSize = 10,
        string? sortBy = null,
        bool sortDescending = false,
        Guid? charityId = null)
    {
        var query = IncludeNavigationProperties()
            .Where(c => !c.IsDeleted);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c =>
                c.Name.Contains(searchTerm) ||
                c.Code.Contains(searchTerm) ||
                c.Email.Contains(searchTerm));
        }

        if (charityId.HasValue)
        {
            query = query.Where(c => c.Id == charityId.Value);
        }

        if (countryId.HasValue)
        {
            query = query.Where(c => c.CountryId == countryId.Value);
        }

        if (regionId.HasValue)
        {
            query = query.Where(c => c.RegionId == regionId.Value);
        }

        if (centerId.HasValue)
        {
            query = query.Where(c => c.CenterId == centerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (isLocked.HasValue)
        {
            query = query.Where(c => c.IsLocked == isLocked.Value);
        }

        if (isAddEnabled.HasValue)
        {
            query = query.Where(c => c.IsAddEnabled == isAddEnabled.Value);
        }

        if (isUpdateEnabled.HasValue)
        {
            query = query.Where(c => c.IsUpdateEnabled == isUpdateEnabled.Value);
        }

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var sortDirection = sortDescending ? "descending" : "ascending";
            try
            {
                query = query.OrderBy($"{sortBy} {sortDirection}");
            }
            catch
            {
                // If sorting fails, default to name
                query = query.OrderBy(c => c.Name);
            }
        }
        else
        {
            query = query.OrderBy(c => c.Name);
        }

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    // Interface-compliant version that takes individual parameters
    public async Task<IEnumerable<Charity>> GetFilteredAsync(string? name = null, string? code = null,
        int? countryId = null, int? regionId = null, int? centerId = null,
        bool? isActive = null, bool? isLocked = null)
    {
        var query = IncludeNavigationProperties()
            .Where(c => !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(c => c.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            query = query.Where(c => c.Code.Contains(code));
        }

        if (countryId.HasValue)
        {
            query = query.Where(c => c.CountryId == countryId.Value);
        }

        if (regionId.HasValue)
        {
            query = query.Where(c => c.RegionId == regionId.Value);
        }

        if (centerId.HasValue)
        {
            query = query.Where(c => c.CenterId == centerId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (isLocked.HasValue)
        {
            query = query.Where(c => c.IsLocked == isLocked.Value);
        }

        return await query.ToListAsync();
    }

    #endregion

    #region Specific Queries

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Name == name && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Email == email && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var query = _dbSet.Where(c => c.Code == code && !c.IsDeleted);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task AddRangeAsync(IEnumerable<Charity> charities)
    {
        await base.InsertRangeAsync(charities);
    }

    public void UpdateRange(IEnumerable<Charity> charities)
    {
        base.UpdateRange(charities);
    }

    public void DeleteRange(IEnumerable<Charity> charities)
    {
        base.DeleteRange(charities);
    }

    #endregion

    #region Statistics

    public async Task<int> GetFamilyCountAsync(Guid charityId)
    {
        return await _context.Set<Family>()
            .CountAsync(f => f.FK_CharityId == charityId && !f.IsDeleted);
    }

    public async Task<int> GetOrphanCountAsync(Guid charityId)
    {
        return await _context.Set<Orphan>()
            .CountAsync(o => o.FK_CharityId == charityId && !o.IsDeleted);
    }

    public async Task<int> GetSponsorCountAsync(Guid charityId)
    {
        return await _context.Set<Sponsor>()
            .CountAsync(s => s.FK_CharityId == charityId && !s.IsDeleted);
    }

    public async Task<(int Total, int Active, int Locked, int ReceivingDonations, int AddedThisMonth,
        List<(int CountryId, string? NameAr, string? NameEn, int Count)> ByCountry)> GetRegisterStatisticsAsync(
        int? countryId = null, Guid? charityId = null)
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var query = _dbSet.AsNoTracking().Where(c => !c.IsDeleted);
        if (charityId.HasValue)
        {
            query = query.Where(c => c.Id == charityId.Value);
        }
        if (countryId.HasValue)
        {
            query = query.Where(c => c.CountryId == countryId.Value);
        }

        // One grouped round-trip for every scalar card; null when the scope matches no rows.
        var totals = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(c => c.IsActive),
                Locked = g.Count(c => c.IsLocked),
                ReceivingDonations = g.Count(c => c.ReceivingDonations),
                AddedThisMonth = g.Count(c => c.CreatedOn >= monthStart)
            })
            .FirstOrDefaultAsync();

        // By-country breakdown — group on the FK then resolve names from the lookup table,
        // which keeps the grouping translatable and authoritative for both languages.
        var byCountryRows = await query
            .Where(c => c.CountryId != null)
            .GroupBy(c => c.CountryId)
            .Select(g => new { CountryId = g.Key!.Value, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        var countryIds = byCountryRows.Select(r => r.CountryId).ToList();
        var countryNames = await _context.Set<Country>()
            .Where(c => countryIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => new { c.NameAr, c.NameEn });

        return (
            totals?.Total ?? 0,
            totals?.Active ?? 0,
            totals?.Locked ?? 0,
            totals?.ReceivingDonations ?? 0,
            totals?.AddedThisMonth ?? 0,
            byCountryRows
                .Select(r =>
                {
                    var names = countryNames.GetValueOrDefault(r.CountryId);
                    return (r.CountryId, names?.NameAr, names?.NameEn, r.Count);
                })
                .ToList());
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<Charity> IncludeNavigationProperties()
    {
        return _dbSet
            .Include(c => c.Country)
            .Include(c => c.Region)
            .Include(c => c.Center)
            .Include(c => c.Bank)
            .Include(c => c.City);
    }

    #endregion
}
