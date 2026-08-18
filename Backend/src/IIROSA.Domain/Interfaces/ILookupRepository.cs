using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Generic interface for lookup repositories
/// Provides common CRUD operations for all lookup entities
/// </summary>
/// <typeparam name="TEntity">Lookup entity type</typeparam>
public interface ILookupRepository<TEntity> : IRepository<TEntity> where TEntity : LookupEntity
{
    // Extended operations for lookups
    Task<IEnumerable<TEntity>> GetActiveAsync();
    Task<bool> ExistsAsync(int id);

    // Filtering operations
    Task<IEnumerable<TEntity>> GetByIsActiveAsync(bool isActive);
    Task<TEntity?> GetByCodeAsync(string code);

    // Ordering operations
    Task<IEnumerable<TEntity>> GetOrderedBySortOrderAsync();
}

/// <summary>
/// Specific interfaces for each lookup entity
/// Add entity-specific methods when needed
/// </summary>
public interface ICountryRepository : ILookupRepository<Country>
{
    Task<Country?> GetByIsoCodeAsync(string isoCode);
    Task<IEnumerable<Country>> GetByCurrencyAsync(string currency);
}

public interface IRegionRepository : ILookupRepository<Region>
{
    Task<IEnumerable<Region>> GetByCountryIdAsync(int countryId);
    Task<Region?> GetByRegionCodeAsync(string regionCode);
}

public interface ICenterRepository : ILookupRepository<Center>
{
    Task<IEnumerable<Center>> GetByRegionIdAsync(int regionId);
    Task<IEnumerable<Center>> GetByCountryIdAsync(int countryId);
    Task<Center?> GetByCenterCodeAsync(string centerCode);
}

public interface IDepartmentRepository : ILookupRepository<Department>
{
    // Department-specific operations can be added here
}

public interface IMissionTypeRepository : ILookupRepository<MissionType>
{
    Task<MissionType?> GetByTypeCodeAsync(string typeCode);
}

public interface IProjectTypeRepository : ILookupRepository<ProjectType>
{
    Task<ProjectType?> GetByTypeCodeAsync(string typeCode);
}

public interface IBankRepository : ILookupRepository<Bank>
{
    Task<Bank?> GetByBankCodeAsync(string bankCode);
    Task<Bank?> GetBySwiftCodeAsync(string swiftCode);
}

public interface INGOTypeRepository : ILookupRepository<NGOType>
{
    Task<NGOType?> GetByTypeCodeAsync(string typeCode);
}
