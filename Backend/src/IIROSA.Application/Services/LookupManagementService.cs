using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Text;
using OfficeOpenXml; // For Excel export

namespace IIROSA.Application.Services;

/// <summary>
/// Generic base service for lookup entities
/// Provides common business operations for all lookup entities
/// </summary>
/// <typeparam name="TEntity">Lookup entity type</typeparam>
/// <typeparam name="TDto">DTO type</typeparam>
/// <typeparam name="TCreateDto">Create DTO type</typeparam>
/// <typeparam name="TUpdateDto">Update DTO type</typeparam>
public class LookupServiceBase<TEntity, TDto, TCreateDto, TUpdateDto> :
    ILookupService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : LookupEntity
    where TDto : class
    where TCreateDto : class
    where TUpdateDto : class
{
    protected readonly ILookupRepository<TEntity> _repository;
    protected readonly IMapper _mapper;
    protected readonly ILogger<LookupServiceBase<TEntity, TDto, TCreateDto, TUpdateDto>> _logger;

    public LookupServiceBase(
        ILookupRepository<TEntity> repository,
        IMapper mapper,
        ILogger<LookupServiceBase<TEntity, TDto, TCreateDto, TUpdateDto>> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public virtual async Task<LookupPagedResult<TDto>> GetLookupItemsAsync(LookupFilterDto filter)
    {
        try
        {
            var allItems = await _repository.GetAllAsync();

            // Apply search filter using reflection to access properties
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchTerm = filter.SearchText.ToLower();
                allItems = allItems.Where(e =>
                    (GetPropertyValue<string>(e, "Name") != null && GetPropertyValue<string>(e, "Name").ToLower().Contains(searchTerm)) ||
                    (GetPropertyValue<string>(e, "NameAr") != null && GetPropertyValue<string>(e, "NameAr").ToLower().Contains(searchTerm)) ||
                    (GetPropertyValue<string>(e, "NameEn") != null && GetPropertyValue<string>(e, "NameEn").ToLower().Contains(searchTerm)));
            }

            // Apply active filter
            if (filter.IsActive.HasValue)
            {
                allItems = allItems.Where(e => GetPropertyValue<bool>(e, "IsActive") == filter.IsActive.Value);
            }

            var totalCount = allItems.Count();

            // Apply pagination using reflection for SortOrder
            var paginatedItems = allItems
                .OrderBy(e => GetPropertyValue<int>(e, "SortOrder"))
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<TDto>>(paginatedItems);

            return new LookupPagedResult<TDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving lookup items");
            throw;
        }
    }

    protected T GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        return property == null ? default : (T)property.GetValue(obj)!;
    }

    public virtual async Task<TDto?> GetLookupByIdAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                return null;

            return _mapper.Map<TDto>(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving lookup item with ID: {LookupId}", id);
            throw;
        }
    }

    public virtual async Task<TDto> CreateLookupAsync(TCreateDto dto)
    {
        try
        {
            var entity = _mapper.Map<TEntity>(dto);
            entity.CreatedOn = DateTime.UtcNow;
            entity.UpdatedOn = DateTime.UtcNow;

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Lookup item created successfully: {LookupId}", entity.Id);

            return await GetLookupByIdAsync(entity.Id) ?? throw new InvalidOperationException("Failed to retrieve created lookup item");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating lookup item");
            throw;
        }
    }

    public virtual async Task<TDto> UpdateLookupAsync(int id, TUpdateDto dto)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Lookup item with ID {id} not found");

            _mapper.Map(dto, entity);
            entity.UpdatedOn = DateTime.UtcNow;

            _repository.Update(entity);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Lookup item updated successfully: {LookupId}", id);

            return await GetLookupByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated lookup item");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating lookup item {LookupId}", id);
            throw;
        }
    }

    public virtual async Task DeleteLookupAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Lookup item with ID {id} not found");

            _repository.Delete(entity);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Lookup item deleted successfully: {LookupId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting lookup item {LookupId}", id);
            throw;
        }
    }

    public virtual async Task ActivateLookupAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Lookup item with ID {id} not found");

            entity.IsActive = true;
            entity.UpdatedOn = DateTime.UtcNow;

            _repository.Update(entity);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Lookup item activated: {LookupId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating lookup item {LookupId}", id);
            throw;
        }
    }

    public virtual async Task DeactivateLookupAsync(int id)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Lookup item with ID {id} not found");

            entity.IsActive = false;
            entity.UpdatedOn = DateTime.UtcNow;

            _repository.Update(entity);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Lookup item deactivated: {LookupId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating lookup item {LookupId}", id);
            throw;
        }
    }

    public virtual async Task UpdateSortOrderAsync(int id, int newSortOrder)
    {
        try
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null)
                throw new InvalidOperationException($"Lookup item with ID {id} not found");

            SetPropertyValue(entity, "SortOrder", newSortOrder);
            SetPropertyValue(entity, "UpdatedOn", DateTime.UtcNow);

            _repository.Update(entity);
            await _repository.SaveChangesAsync();

            _logger.LogInformation("Lookup item sort order updated: {LookupId} -> {SortOrder}", id, newSortOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating sort order for lookup item {LookupId}", id);
            throw;
        }
    }

    public virtual async Task ReorderLookupItemsAsync(Dictionary<int, int> sortOrderUpdates)
    {
        try
        {
            foreach (var (id, sortOrder) in sortOrderUpdates)
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity != null)
                {
                    SetPropertyValue(entity, "SortOrder", sortOrder);
                    SetPropertyValue(entity, "UpdatedOn", DateTime.UtcNow);
                    _repository.Update(entity);
                }
            }

            await _repository.SaveChangesAsync();
            _logger.LogInformation("Bulk reorder completed for {Count} lookup items", sortOrderUpdates.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during bulk reorder of lookup items");
            throw;
        }
    }

    protected void SetPropertyValue(object obj, string propertyName, object value)
    {
        var property = obj.GetType().GetProperty(propertyName);
        property?.SetValue(obj, value);
    }

    public virtual async Task<byte[]> ExportLookupAsync(BulkExportDto exportDto)
    {
        try
        {
            var allItems = await _repository.GetAllAsync();

            if (!exportDto.IncludeInactive)
            {
                allItems = allItems.Where(x => x.IsActive);
            }

            var dtos = _mapper.Map<List<TDto>>(allItems);

            // For now, implement CSV export
            // TODO: Implement Excel export using EPPlus or similar library
            return await ExportToCsvAsync(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting lookup items");
            throw;
        }
    }

    public virtual async Task<BulkImportResultDto> ImportLookupAsync(BulkImportDto importDto)
    {
        try
        {
            var result = new BulkImportResultDto();

            foreach (var itemDto in importDto.Items)
            {
                try
                {
                    var entity = _mapper.Map<TEntity>(itemDto);
                    entity.CreatedOn = DateTime.UtcNow;
                    entity.UpdatedOn = DateTime.UtcNow;

                    await _repository.AddAsync(entity);
                    result.Successful++;
                }
                catch (Exception ex)
                {
                    result.Failed++;
                    result.Errors.Add($"Error importing item: {ex.Message}");
                    _logger.LogWarning(ex, "Failed to import lookup item during bulk import");
                }
            }

            await _repository.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during bulk import of lookup items");
            throw;
        }
    }

    protected virtual async Task<byte[]> ExportToCsvAsync(List<TDto> items)
    {
        // Basic CSV export implementation
        var csv = new StringBuilder();

        // Get properties and create header
        var properties = typeof(TDto).GetProperties();
        csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Add data rows
        foreach (var item in items)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return value?.ToString()?.Replace(",", "") ?? "";
            });
            csv.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}

/// <summary>
/// Specific service implementations for each lookup type
/// </summary>
public class CountryService : LookupServiceBase<Country, CountryDto, CreateCountryDto, UpdateCountryDto>, ICountryService
{
    private readonly ICountryRepository _countryRepository;

    public CountryService(
        ICountryRepository repository,
        IMapper mapper,
        ILogger<CountryService> logger)
        : base(repository, mapper, logger)
    {
        _countryRepository = repository;
    }

    public async Task<List<CountryDto>> GetCountriesByCurrencyAsync(string currency)
    {
        var countries = await _countryRepository.GetByCurrencyAsync(currency);
        return _mapper.Map<List<CountryDto>>(countries);
    }

    public async Task<CountryDto?> GetCountryByIsoCodeAsync(string isoCode)
    {
        var country = await _countryRepository.GetByIsoCodeAsync(isoCode);
        return country == null ? null : _mapper.Map<CountryDto>(country);
    }
}

public class RegionService : LookupServiceBase<Region, RegionDto, CreateRegionDto, UpdateRegionDto>, IRegionService
{
    private readonly IRegionRepository _regionRepository;

    public RegionService(
        IRegionRepository repository,
        IMapper mapper,
        ILogger<RegionService> logger)
        : base(repository, mapper, logger)
    {
        _regionRepository = repository;
    }

    public override async Task<LookupPagedResult<RegionDto>> GetLookupItemsAsync(LookupFilterDto filter)
    {
        try
        {
            var allItems = await _repository.GetAllAsync();

            // Filter by countryId if provided
            if (filter.CountryId.HasValue)
            {
                allItems = allItems.Where(e => e.CountryId == filter.CountryId.Value);
            }

            // Apply search filter using reflection to access properties
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchTerm = filter.SearchText.ToLower();
                allItems = allItems.Where(e =>
                    (GetPropertyValue<string>(e, "Name") != null && GetPropertyValue<string>(e, "Name").ToLower().Contains(searchTerm)) ||
                    (GetPropertyValue<string>(e, "NameAr") != null && GetPropertyValue<string>(e, "NameAr").ToLower().Contains(searchTerm)) ||
                    (GetPropertyValue<string>(e, "NameEn") != null && GetPropertyValue<string>(e, "NameEn").ToLower().Contains(searchTerm)));
            }

            // Apply active filter
            if (filter.IsActive.HasValue)
            {
                allItems = allItems.Where(e => GetPropertyValue<bool>(e, "IsActive") == filter.IsActive.Value);
            }

            var totalCount = allItems.Count();

            // Apply pagination using reflection for SortOrder
            var paginatedItems = allItems
                .OrderBy(e => GetPropertyValue<int>(e, "SortOrder"))
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<RegionDto>>(paginatedItems);

            return new LookupPagedResult<RegionDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving regions");
            throw;
        }
    }

    public async Task<List<RegionDto>> GetRegionsByCountryAsync(int countryId)
    {
        var regions = await _regionRepository.GetByCountryIdAsync(countryId);
        return _mapper.Map<List<RegionDto>>(regions);
    }
}

public class CenterService : LookupServiceBase<Center, CenterDto, CreateCenterDto, UpdateCenterDto>, ICenterService
{
    private readonly ICenterRepository _centerRepository;

    public CenterService(
        ICenterRepository repository,
        IMapper mapper,
        ILogger<CenterService> logger)
        : base(repository, mapper, logger)
    {
        _centerRepository = repository;
    }

    public override async Task<LookupPagedResult<CenterDto>> GetLookupItemsAsync(LookupFilterDto filter)
    {
        try
        {
            var allItems = await _repository.GetAllAsync();

            // Filter by regionId if provided
            if (filter.RegionId.HasValue)
            {
                allItems = allItems.Where(e => e.RegionId == filter.RegionId.Value);
            }

            // Filter by countryId if provided
            if (filter.CountryId.HasValue)
            {
                allItems = allItems.Where(e => e.CountryId == filter.CountryId.Value);
            }

            // Apply search filter using reflection to access properties
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchTerm = filter.SearchText.ToLower();
                allItems = allItems.Where(e =>
                    (GetPropertyValue<string>(e, "Name") != null && GetPropertyValue<string>(e, "Name").ToLower().Contains(searchTerm)) ||
                    (GetPropertyValue<string>(e, "NameAr") != null && GetPropertyValue<string>(e, "NameAr").ToLower().Contains(searchTerm)) ||
                    (GetPropertyValue<string>(e, "NameEn") != null && GetPropertyValue<string>(e, "NameEn").ToLower().Contains(searchTerm)));
            }

            // Apply active filter
            if (filter.IsActive.HasValue)
            {
                allItems = allItems.Where(e => GetPropertyValue<bool>(e, "IsActive") == filter.IsActive.Value);
            }

            var totalCount = allItems.Count();

            // Apply pagination using reflection for SortOrder
            var paginatedItems = allItems
                .OrderBy(e => GetPropertyValue<int>(e, "SortOrder"))
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<CenterDto>>(paginatedItems);

            return new LookupPagedResult<CenterDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving centers");
            throw;
        }
    }

    public async Task<List<CenterDto>> GetCentersByRegionAsync(int regionId)
    {
        var centers = await _centerRepository.GetByRegionIdAsync(regionId);
        return _mapper.Map<List<CenterDto>>(centers);
    }

    public async Task<List<CenterDto>> GetCentersByCountryAsync(int countryId)
    {
        var centers = await _centerRepository.GetByCountryIdAsync(countryId);
        return _mapper.Map<List<CenterDto>>(centers);
    }
}

public class DepartmentService : LookupServiceBase<Department, DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto>, IDepartmentService
{
    public DepartmentService(
        IDepartmentRepository repository,
        IMapper mapper,
        ILogger<DepartmentService> logger)
        : base(repository, mapper, logger)
    {
    }
}

public class MissionTypeService : LookupServiceBase<MissionType, MissionTypeDto, CreateMissionTypeDto, UpdateMissionTypeDto>, IMissionTypeService
{
    public MissionTypeService(
        IMissionTypeRepository repository,
        IMapper mapper,
        ILogger<MissionTypeService> logger)
        : base(repository, mapper, logger)
    {
    }
}

public class ProjectTypeService : LookupServiceBase<ProjectType, ProjectTypeDto, CreateProjectTypeDto, UpdateProjectTypeDto>, IProjectTypeService
{
    public ProjectTypeService(
        IProjectTypeRepository repository,
        IMapper mapper,
        ILogger<ProjectTypeService> logger)
        : base(repository, mapper, logger)
    {
    }
}

public class BankService : LookupServiceBase<Bank, BankDto, CreateBankDto, UpdateBankDto>, IBankService
{
    public BankService(
        IBankRepository repository,
        IMapper mapper,
        ILogger<BankService> logger)
        : base(repository, mapper, logger)
    {
    }
}

public class NGOTypeService : LookupServiceBase<NGOType, NGOTypeDto, CreateNGOTypeDto, UpdateNGOTypeDto>, INGOTypeService
{
    public NGOTypeService(
        INGOTypeRepository repository,
        IMapper mapper,
        ILogger<NGOTypeService> logger)
        : base(repository, mapper, logger)
    {
    }
}