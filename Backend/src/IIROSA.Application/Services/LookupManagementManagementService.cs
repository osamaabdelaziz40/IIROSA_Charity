using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Lookup management service for UC-14.5: View All Lookup Tables
/// Provides summary and management operations for all lookup tables
/// </summary>
public class LookupManagementService : ILookupManagementService
{
    private readonly ICountryRepository _countryRepository;
    private readonly IRegionRepository _regionRepository;
    private readonly ICenterRepository _centerRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IMissionTypeRepository _missionTypeRepository;
    private readonly IProjectTypeRepository _projectTypeRepository;
    private readonly IBankRepository _bankRepository;
    private readonly INGOTypeRepository _ngoTypeRepository;
    private readonly ILogger<LookupManagementService> _logger;

    public LookupManagementService(
        ICountryRepository countryRepository,
        IRegionRepository regionRepository,
        ICenterRepository centerRepository,
        IDepartmentRepository departmentRepository,
        IMissionTypeRepository missionTypeRepository,
        IProjectTypeRepository projectTypeRepository,
        IBankRepository bankRepository,
        INGOTypeRepository ngoTypeRepository,
        ILogger<LookupManagementService> logger)
    {
        _countryRepository = countryRepository;
        _regionRepository = regionRepository;
        _centerRepository = centerRepository;
        _departmentRepository = departmentRepository;
        _missionTypeRepository = missionTypeRepository;
        _projectTypeRepository = projectTypeRepository;
        _bankRepository = bankRepository;
        _ngoTypeRepository = ngoTypeRepository;
        _logger = logger;
    }

    public async Task<List<LookupTableSummaryDto>> GetAllLookupTablesSummaryAsync()
    {
        try
        {
            var summaries = new List<LookupTableSummaryDto>();

            // Get summary for each lookup table using the repositories
            summaries.Add(await GetTableSummaryAsync("Countries", "Countries", "البلدان", _countryRepository));
            summaries.Add(await GetTableSummaryAsync("Regions", "Regions", "المناطق", _regionRepository));
            summaries.Add(await GetTableSummaryAsync("Centers", "Centers", "المراكز", _centerRepository));
            summaries.Add(await GetTableSummaryAsync("Departments", "Departments", "الأقسام", _departmentRepository));
            summaries.Add(await GetTableSummaryAsync("MissionTypes", "Mission Types", "أنواع المهام", _missionTypeRepository));
            summaries.Add(await GetTableSummaryAsync("ProjectTypes", "Project Types", "أنواع المشاريع", _projectTypeRepository));
            summaries.Add(await GetTableSummaryAsync("Banks", "Banks", "البنوك", _bankRepository));
            summaries.Add(await GetTableSummaryAsync("NGOTypes", "NGO Types", "أنواع الجمعيات", _ngoTypeRepository));

            return summaries.OrderByDescending(s => s.LastModified).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting lookup tables summary");
            throw;
        }
    }

    public async Task<byte[]> ExportLookupTableAsync(string tableName, BulkExportDto exportDto)
    {
        try
        {
            return tableName switch
            {
                "Countries" => await ExportTableAsync(_countryRepository, exportDto),
                "Regions" => await ExportTableAsync(_regionRepository, exportDto),
                "Centers" => await ExportTableAsync(_centerRepository, exportDto),
                "Departments" => await ExportTableAsync(_departmentRepository, exportDto),
                "MissionTypes" => await ExportTableAsync(_missionTypeRepository, exportDto),
                "ProjectTypes" => await ExportTableAsync(_projectTypeRepository, exportDto),
                "Banks" => await ExportTableAsync(_bankRepository, exportDto),
                "NGOTypes" => await ExportTableAsync(_ngoTypeRepository, exportDto),
                _ => throw new ArgumentException($"Unknown lookup table: {tableName}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting lookup table: {TableName}", tableName);
            throw;
        }
    }

    public async Task<BulkImportResultDto> ImportLookupTableAsync(string tableName, BulkImportDto importDto)
    {
        try
        {
            // TODO: Implement bulk import functionality
            // This would parse the import data and create/update lookup items
            throw new NotImplementedException("Bulk import functionality will be implemented in a future update");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while importing lookup table: {TableName}", tableName);
            throw;
        }
    }

    private async Task<LookupTableSummaryDto> GetTableSummaryAsync<TEntity>(string tableName, string displayName, string displayNameAr, ILookupRepository<TEntity> repository) where TEntity : LookupEntity
    {
        var allItems = await repository.GetAllAsync();
        var totalCount = allItems.Count();
        var activeCount = allItems.Count(e => e.IsActive);
        var inactiveCount = totalCount - activeCount;

        // Get the last modified date using reflection to access UpdatedOn property
        var lastModified = allItems
            .OrderByDescending(e => GetPropertyValue<DateTime>(e, "CreatedOn"))
            .Select(e => (DateTime?)GetPropertyValue<DateTime>(e, "CreatedOn"))
            .FirstOrDefault();

        return new LookupTableSummaryDto
        {
            TableName = tableName,
            TableDisplayName = $"{displayNameAr} - {displayName}",
            ItemCount = totalCount,
            ActiveItems = activeCount,
            InactiveItems = inactiveCount,
            LastModified = lastModified
        };
    }

    private T GetPropertyValue<T>(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(propertyName);
        return property == null ? default : (T)property.GetValue(obj)!;
    }

    private async Task<byte[]> ExportTableAsync<TEntity>(ILookupRepository<TEntity> repository, BulkExportDto exportDto) where TEntity : LookupEntity
    {
        var allItems = await repository.GetAllAsync();

        if (!exportDto.IncludeInactive)
        {
            // Use reflection to access IsActive property
            allItems = allItems.Where(x => GetPropertyValue<bool>(x, "IsActive"));
        }

        // TODO: Implement proper Excel/CSV export based on format
        // For now, return a simple CSV
        var csv = new System.Text.StringBuilder();

        // Header
        var properties = typeof(TEntity).GetProperties();
        csv.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Data rows
        foreach (var item in allItems)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                return value?.ToString()?.Replace(",", "") ?? "";
            });
            csv.AppendLine(string.Join(",", values));
        }

        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }
}