using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Generic repository implementation for lookup entities
/// Provides common CRUD operations for all lookup entities
/// </summary>
/// <typeparam name="TEntity">Lookup entity type</typeparam>
public class LookupRepository<TEntity> : Repository<TEntity>, ILookupRepository<TEntity> where TEntity : LookupEntity
{
    protected readonly DbSet<TEntity> _dbSet;

    public LookupRepository(ApplicationDbContext context) : base(context)
    {
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> GetActiveAsync()
    {
        return await _dbSet
            .Where(e => e.IsActive)
            .OrderBy(e => e.SortOrder)
            .ToListAsync();
    }

    public virtual async Task<bool> ExistsAsync(int id)
    {
        return await _dbSet.AnyAsync(e => e.Id == id);
    }

    public virtual async Task<IEnumerable<TEntity>> GetByIsActiveAsync(bool isActive)
    {
        return await _dbSet
            .Where(e => e.IsActive == isActive)
            .OrderBy(e => e.SortOrder)
            .ToListAsync();
    }

    public virtual async Task<TEntity?> GetByCodeAsync(string code)
    {
        // This method should be overridden in specific repositories if they have a Code property
        return await _dbSet.FirstOrDefaultAsync(e => e.Name == code);
    }

    public virtual async Task<IEnumerable<TEntity>> GetOrderedBySortOrderAsync()
    {
        return await _dbSet
            .OrderBy(e => e.SortOrder)
            .ToListAsync();
    }
}

/// <summary>
/// Specific repository implementations for each lookup entity
/// </summary>
public class CountryRepository : LookupRepository<Country>, ICountryRepository
{
    public CountryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Country?> GetByIsoCodeAsync(string isoCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(c => c.IsoCode == isoCode);
    }

    public async Task<IEnumerable<Country>> GetByCurrencyAsync(string currency)
    {
        return await _dbSet
            .Where(c => c.Currency == currency)
            .ToListAsync();
    }
}

public class RegionRepository : LookupRepository<Region>, IRegionRepository
{
    public RegionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Region>> GetByCountryIdAsync(int countryId)
    {
        return await _dbSet
            .Include(r => r.Country)
            .Where(r => r.CountryId == countryId)
            .OrderBy(r => r.SortOrder)
            .ToListAsync();
    }

    public async Task<Region?> GetByRegionCodeAsync(string regionCode)
    {
        return await _dbSet
            .Include(r => r.Country)
            .FirstOrDefaultAsync(r => r.RegionCode == regionCode);
    }
}

public class CenterRepository : LookupRepository<Center>, ICenterRepository
{
    public CenterRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Center>> GetByRegionIdAsync(int regionId)
    {
        return await _dbSet
            .Include(c => c.Region)
            .Include(c => c.Country)
            .Where(c => c.RegionId == regionId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<Center>> GetByCountryIdAsync(int countryId)
    {
        return await _dbSet
            .Include(c => c.Region)
            .Include(c => c.Country)
            .Where(c => c.CountryId == countryId)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<Center?> GetByCenterCodeAsync(string centerCode)
    {
        return await _dbSet
            .Include(c => c.Region)
            .Include(c => c.Country)
            .FirstOrDefaultAsync(c => c.CenterCode == centerCode);
    }
}

public class DepartmentRepository : LookupRepository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class MissionTypeRepository : LookupRepository<MissionType>, IMissionTypeRepository
{
    public MissionTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MissionType?> GetByTypeCodeAsync(string typeCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.TypeCode == typeCode);
    }
}

public class ProjectTypeRepository : LookupRepository<ProjectType>, IProjectTypeRepository
{
    public ProjectTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ProjectType?> GetByTypeCodeAsync(string typeCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.TypeCode == typeCode);
    }
}

public class BankRepository : LookupRepository<Bank>, IBankRepository
{
    public BankRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Bank?> GetByBankCodeAsync(string bankCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.BankCode == bankCode);
    }

    public async Task<Bank?> GetBySwiftCodeAsync(string swiftCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(b => b.SwiftCode == swiftCode);
    }
}

public class NGOTypeRepository : LookupRepository<NGOType>, INGOTypeRepository
{
    public NGOTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<NGOType?> GetByTypeCodeAsync(string typeCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(n => n.TypeCode == typeCode);
    }
}