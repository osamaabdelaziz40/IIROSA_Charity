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

public interface IProjectTypeService : ILookupService<ProjectTypeDto, ProjectTypeDto, CreateProjectTypeDto, UpdateProjectTypeDto>
{
}

public interface IBankService : ILookupService<BankDto, BankDto, CreateBankDto, UpdateBankDto>
{
}

public interface INGOTypeService : ILookupService<NGOTypeDto, NGOTypeDto, CreateNGOTypeDto, UpdateNGOTypeDto>
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
}