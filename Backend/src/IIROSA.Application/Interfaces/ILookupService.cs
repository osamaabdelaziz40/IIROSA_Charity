using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Generic interface for lookup services
/// Provides common business operations for all lookup entities
/// </summary>
public interface ILookupService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : class
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    // Basic CRUD operations
    Task<LookupPagedResult<TDto>> GetLookupItemsAsync(LookupFilterDto filter);
    Task<TDto?> GetLookupByIdAsync(int id);
    Task<TDto> CreateLookupAsync(TCreateDto dto);
    Task<TDto> UpdateLookupAsync(int id, TUpdateDto dto);
    Task DeleteLookupAsync(int id);

    // Status operations
    Task ActivateLookupAsync(int id);
    Task DeactivateLookupAsync(int id);

    // Ordering operations
    Task UpdateSortOrderAsync(int id, int newSortOrder);
    Task ReorderLookupItemsAsync(Dictionary<int, int> sortOrderUpdates);

    // Export/Import operations
    Task<byte[]> ExportLookupAsync(BulkExportDto exportDto);
    Task<BulkImportResultDto> ImportLookupAsync(BulkImportDto importDto);
}

/// <summary>
/// Specific interfaces for each lookup entity service
/// </summary>
public interface ICountryService : ILookupService<CountryDto, CountryDto, CreateCountryDto, UpdateCountryDto>
{
    Task<List<CountryDto>> GetCountriesByCurrencyAsync(string currency);
    Task<CountryDto?> GetCountryByIsoCodeAsync(string isoCode);
}

public interface IRegionService : ILookupService<RegionDto, RegionDto, CreateRegionDto, UpdateRegionDto>
{
    Task<List<RegionDto>> GetRegionsByCountryAsync(int countryId);
}

public interface ICenterService : ILookupService<CenterDto, CenterDto, CreateCenterDto, UpdateCenterDto>
{
    Task<List<CenterDto>> GetCentersByRegionAsync(int regionId);
    Task<List<CenterDto>> GetCentersByCountryAsync(int countryId);
}

public interface IDepartmentService : ILookupService<DepartmentDto, DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto>
{
}

public interface IMissionTypeService : ILookupService<MissionTypeDto, MissionTypeDto, CreateMissionTypeDto, UpdateMissionTypeDto>
{
}

public interface IMissionInterviewTypeService : ILookupService<MissionInterviewTypeDto, MissionInterviewTypeDto, CreateMissionInterviewTypeDto, UpdateMissionInterviewTypeDto>
{
}

public interface IMissionTimeTypeService : ILookupService<MissionTimeTypeDto, MissionTimeTypeDto, CreateMissionTimeTypeDto, UpdateMissionTimeTypeDto>
{
}

public interface IProjectTypeService : ILookupService<ProjectTypeDto, ProjectTypeDto, CreateProjectTypeDto, UpdateProjectTypeDto>
{
}

public interface IBankService : ILookupService<BankDto, BankDto, CreateBankDto, UpdateBankDto>
{
}

public interface INGOTypeService : ILookupService<NGOTypeDto, NGOTypeDto, CreateNGOTypeDto, UpdateNGOTypeDto>
{
}

public interface IEducationLevelService : ILookupService<EducationLevelDto, EducationLevelDto, CreateEducationLevelDto, UpdateEducationLevelDto>
{
}

public interface IHealthStatusService : ILookupService<HealthStatusDto, HealthStatusDto, CreateHealthStatusDto, UpdateHealthStatusDto>
{
}

public interface IRefuseReasonService : ILookupService<RefuseReasonDto, RefuseReasonDto, CreateRefuseReasonDto, UpdateRefuseReasonDto>
{
}

// Refugee register lookups (epic 7, UC-REF-03) — refugee-form dropdowns

public interface IHouseOwnershipService : ILookupService<HouseOwnershipDto, HouseOwnershipDto, CreateHouseOwnershipDto, UpdateHouseOwnershipDto>
{
}

public interface IHouseStatusService : ILookupService<HouseStatusDto, HouseStatusDto, CreateHouseStatusDto, UpdateHouseStatusDto>
{
}

public interface IIncomeTypeService : ILookupService<IncomeTypeDto, IncomeTypeDto, CreateIncomeTypeDto, UpdateIncomeTypeDto>
{
}

public interface ISocialStatusService : ILookupService<SocialStatusDto, SocialStatusDto, CreateSocialStatusDto, UpdateSocialStatusDto>
{
}

public interface IRelationService : ILookupService<RelationDto, RelationDto, CreateRelationDto, UpdateRelationDto>
{
}

public interface IReasonOfRelService : ILookupService<ReasonOfRelDto, ReasonOfRelDto, CreateReasonOfRelDto, UpdateReasonOfRelDto>
{
}

/// <summary>نوع السكن — shared catalogue (§12.S.2 refugee form)</summary>
public interface IHousingTypeService : ILookupService<HousingTypeDto, HousingTypeDto, CreateHousingTypeDto, UpdateHousingTypeDto>
{
}

/// <summary>Guardian marital status — الحالة الاجتماعية للعائل (UC-SYS-05, epic 19)</summary>
public interface IMaritalStatusService : ILookupService<MaritalStatusDto, MaritalStatusDto, CreateMaritalStatusDto, UpdateMaritalStatusDto>
{
}
/// <summary>Guardian job / profession — المهنة (UC-SYS-09, epic 19)</summary>
public interface IJobService : ILookupService<JobDto, JobDto, CreateJobDto, UpdateJobDto>
{
}

/// <summary>
/// Lookup management service for UC-14.5: View All Lookup Tables
/// </summary>
public interface ILookupManagementService
{
    Task<List<LookupTableSummaryDto>> GetAllLookupTablesSummaryAsync();
    Task<byte[]> ExportLookupTableAsync(string tableName, BulkExportDto exportDto);
    Task<BulkImportResultDto> ImportLookupTableAsync(string tableName, BulkImportDto importDto);

    /// <summary>
    /// UC-CHQ-05 — cheque beneficiary type-ahead for the cheque form (§16.S.2).
    /// </summary>
    Task<List<ChequeBeneficiaryOptionDto>> GetChequeBeneficiariesAsync(string? term, int take = 20);

    /// <summary>
    /// UC-CHQ-06 — the distinct currencies configured on countries, for the cheque form.
    /// </summary>
    Task<List<CurrencyOptionDto>> GetCurrenciesAsync();

    /// <summary>
    /// UC-CHQ-08 — a bank's cheque stationery print offsets (mm from the leaf's top-right).
    /// </summary>
    Task<BankChequePositionsDto> GetBankChequePositionsAsync(int bankId);
}