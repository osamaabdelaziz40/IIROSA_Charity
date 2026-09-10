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

public interface IMissionInterviewTypeRepository : ILookupRepository<MissionInterviewType>
{
}

public interface IMissionTimeTypeRepository : ILookupRepository<MissionTimeType>
{
    Task<MissionTimeType?> GetByTimeTypeCodeAsync(string timeTypeCode);
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

public interface IOutgoingCategoryRepository : ILookupRepository<OutgoingCategory>
{
    // Outgoing letter categories (epic 16, UC-COR-14) — GetActiveAsync covers the dropdown
}

public interface IEducationLevelRepository : ILookupRepository<EducationLevel>
{
    // Orphan reference data (UC-ORP-11) — GetActiveAsync covers the orphan-form dropdown
}

public interface IHealthStatusRepository : ILookupRepository<HealthStatus>
{
    // Orphan reference data (UC-ORP-11) — GetActiveAsync covers the orphan-form dropdown
}

public interface IDeathReasonRepository : ILookupRepository<DeathReason>
{
    // Cause-of-death catalogue (طبيعية / مرض / حادث) — GetActiveAsync covers the father/mother death-reason dropdowns
}

public interface IRefuseReasonRepository : ILookupRepository<RefuseReason>
{
    // Periodic report refusal catalogue (epic 9, UC-ORR-08) — GetActiveAsync covers the refusal drop-down
}

// Refugee register lookups (epic 7, UC-REF-03) — GetActiveAsync covers the refugee-form dropdowns

public interface IHouseOwnershipRepository : ILookupRepository<HouseOwnership>
{
}

public interface IHouseStatusRepository : ILookupRepository<HouseStatus>
{
}

public interface IIncomeTypeRepository : ILookupRepository<IncomeType>
{
}

public interface ISocialStatusRepository : ILookupRepository<SocialStatus>
{
}

public interface IRelationRepository : ILookupRepository<Relation>
{
}

public interface IReasonOfRelRepository : ILookupRepository<ReasonOfRel>
{
}

/// <summary>نوع السكن — shared catalogue (§12.S.2 refugee form / legacy Family.HousingTypeId)</summary>
public interface IHousingTypeRepository : ILookupRepository<HousingType>
{
}

/// <summary>حالة المشروع — family data extension catalogue (يوجد مشروع قائم / مشروع جديد)</summary>
public interface IFamilyProjectStatusRepository : ILookupRepository<FamilyProjectStatus>
{
}

/// <summary>Guardian marital status — الحالة الاجتماعية للعائل (UC-SYS-05, epic 19)</summary>
public interface IMaritalStatusRepository : ILookupRepository<MaritalStatus>
{
}
/// <summary>Guardian job / profession — المهنة (UC-SYS-09, epic 19)</summary>
public interface IJobRepository : ILookupRepository<Job>
{
}
