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

public class EducationLevelRepository : LookupRepository<EducationLevel>, IEducationLevelRepository
{
    public EducationLevelRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class HealthStatusRepository : LookupRepository<HealthStatus>, IHealthStatusRepository
{
    public HealthStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class DeathReasonRepository : LookupRepository<DeathReason>, IDeathReasonRepository
{
    public DeathReasonRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class RefuseReasonRepository : LookupRepository<RefuseReason>, IRefuseReasonRepository
{
    public RefuseReasonRepository(ApplicationDbContext context) : base(context)
    {
    }
}

// Refugee register lookups (epic 7, UC-REF-03)

public class HouseOwnershipRepository : LookupRepository<HouseOwnership>, IHouseOwnershipRepository
{
    public HouseOwnershipRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class HouseStatusRepository : LookupRepository<HouseStatus>, IHouseStatusRepository
{
    public HouseStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class IncomeTypeRepository : LookupRepository<IncomeType>, IIncomeTypeRepository
{
    public IncomeTypeRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class SocialStatusRepository : LookupRepository<SocialStatus>, ISocialStatusRepository
{
    public SocialStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class RelationRepository : LookupRepository<Relation>, IRelationRepository
{
    public RelationRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class ReasonOfRelRepository : LookupRepository<ReasonOfRel>, IReasonOfRelRepository
{
    public ReasonOfRelRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class HousingTypeRepository : LookupRepository<HousingType>, IHousingTypeRepository
{
    public HousingTypeRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class FamilyProjectStatusRepository : LookupRepository<FamilyProjectStatus>, IFamilyProjectStatusRepository
{
    public FamilyProjectStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class MaritalStatusRepository : LookupRepository<MaritalStatus>, IMaritalStatusRepository
{
    public MaritalStatusRepository(ApplicationDbContext context) : base(context)
    {
    }
}
public class JobRepository : LookupRepository<Job>, IJobRepository
{
    public JobRepository(ApplicationDbContext context) : base(context)
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

public class MissionInterviewTypeRepository : LookupRepository<MissionInterviewType>, IMissionInterviewTypeRepository
{
    public MissionInterviewTypeRepository(ApplicationDbContext context) : base(context)
    {
    }
}

public class MissionTimeTypeRepository : LookupRepository<MissionTimeType>, IMissionTimeTypeRepository
{
    public MissionTimeTypeRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<MissionTimeType?> GetByTimeTypeCodeAsync(string timeTypeCode)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.TimeTypeCode == timeTypeCode);
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

/// <summary>
/// Outgoing letter category lookup repository (epic 16, UC-COR-14)
/// </summary>
public class OutgoingCategoryRepository : LookupRepository<OutgoingCategory>, IOutgoingCategoryRepository
{
    public OutgoingCategoryRepository(ApplicationDbContext context) : base(context)
    {
    }
}

/// <summary>
/// Housing building lookup repository (epic 6) — auto-registers ILookupRepository&lt;HousingBuilding&gt;
/// </summary>
public class HousingBuildingRepository : LookupRepository<HousingBuilding>
{
    public HousingBuildingRepository(ApplicationDbContext context) : base(context)
    {
    }
}

/// <summary>
/// Housing flat lookup repository (epic 6) — auto-registers ILookupRepository&lt;HousingFlat&gt;
/// </summary>
public class HousingFlatRepository : LookupRepository<HousingFlat>
{
    public HousingFlatRepository(ApplicationDbContext context) : base(context)
    {
    }
}