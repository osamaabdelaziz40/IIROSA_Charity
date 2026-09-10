using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
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
    private readonly ILookupRepository<OfficeProjectType> _officeProjectTypeRepository;
    private readonly ILookupRepository<HousingBuilding> _housingBuildingRepository;
    private readonly ILookupRepository<HousingFlat> _housingFlatRepository;
    private readonly IOutgoingCategoryRepository _outgoingCategoryRepository;
    private readonly IRepository<ChequeBeneficiary> _chequeBeneficiaryRepository;
    private readonly IHouseOwnershipRepository _houseOwnershipRepository;
    private readonly IIncomeTypeRepository _incomeTypeRepository;
    private readonly IFamilyProjectStatusRepository _familyProjectStatusRepository;
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
        ILookupRepository<OfficeProjectType> officeProjectTypeRepository,
        ILookupRepository<HousingBuilding> housingBuildingRepository,
        ILookupRepository<HousingFlat> housingFlatRepository,
        IOutgoingCategoryRepository outgoingCategoryRepository,
        IRepository<ChequeBeneficiary> chequeBeneficiaryRepository,
        IHouseOwnershipRepository houseOwnershipRepository,
        IIncomeTypeRepository incomeTypeRepository,
        IFamilyProjectStatusRepository familyProjectStatusRepository,
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
        _officeProjectTypeRepository = officeProjectTypeRepository;
        _housingBuildingRepository = housingBuildingRepository;
        _housingFlatRepository = housingFlatRepository;
        _outgoingCategoryRepository = outgoingCategoryRepository;
        _chequeBeneficiaryRepository = chequeBeneficiaryRepository;
        _houseOwnershipRepository = houseOwnershipRepository;
        _incomeTypeRepository = incomeTypeRepository;
        _familyProjectStatusRepository = familyProjectStatusRepository;
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
            summaries.Add(await GetTableSummaryAsync("OfficeProjectTypes", "Office Development Project Types", "أنواع مشاريع تطوير المكاتب", _officeProjectTypeRepository));
            summaries.Add(await GetTableSummaryAsync("HousingBuildings", "Housing Buildings", "عمارات الإسكان", _housingBuildingRepository));
            summaries.Add(await GetTableSummaryAsync("HousingFlats", "Housing Flats", "شقق الإسكان", _housingFlatRepository));
            summaries.Add(await GetTableSummaryAsync("OutgoingCategories", "Outgoing Categories", "تصنيفات الصادر", _outgoingCategoryRepository));
            // Family data extension catalogues (§4 معلومات الأسرة)
            summaries.Add(await GetTableSummaryAsync("HouseOwnerships", "House Ownerships", "ملكية السكن", _houseOwnershipRepository));
            summaries.Add(await GetTableSummaryAsync("IncomeTypes", "Income Types", "أنواع الدخل", _incomeTypeRepository));
            summaries.Add(await GetTableSummaryAsync("FamilyProjectStatuses", "Family Project Statuses", "حالة المشروع", _familyProjectStatusRepository));

            return summaries.OrderByDescending(s => s.LastModified).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting lookup tables summary");
            throw;
        }
    }

    /// <summary>
    /// UC-CHQ-05 — active cheque beneficiaries for the cheque-form type-ahead,
    /// matching Arabic or English names.
    /// </summary>
    public async Task<List<ChequeBeneficiaryOptionDto>> GetChequeBeneficiariesAsync(string? term, int take = 20)
    {
        var query = _chequeBeneficiaryRepository.AsQueryable()
            .Where(b => b.IsActive);

        if (!string.IsNullOrWhiteSpace(term))
        {
            var trimmed = term.Trim();
            query = query.Where(b => b.NameAr.Contains(trimmed) || b.NameEn.Contains(trimmed));
        }

        var beneficiaries = await query
            .OrderBy(b => b.NameAr)
            .Take(Math.Clamp(take, 1, 50))
            .ToListAsync();

        return beneficiaries.Select(b => new ChequeBeneficiaryOptionDto
        {
            Id = b.Id,
            Name = b.Name,
            NameAr = b.NameAr,
            NameEn = b.NameEn,
            BeneficiaryType = b.BeneficiaryType,
            Address = b.Address,
            Phone = b.Phone,
            Email = b.Email,
            IdNumber = b.IdNumber,
            BankId = b.FK_BankId,
            AccountNumber = b.AccountNumber
        }).ToList();
    }

    /// <summary>
    /// UC-CHQ-06 — the distinct currencies configured on countries, for the cheque form.
    /// </summary>
    public async Task<List<CurrencyOptionDto>> GetCurrenciesAsync()
    {
        var countries = await _countryRepository.GetAllAsync();

        return countries
            .Where(c => !string.IsNullOrWhiteSpace(c.Currency))
            .GroupBy(c => c.Currency!.Trim().ToUpperInvariant())
            .Select(g => new CurrencyOptionDto
            {
                Code = g.Key,
                NameAr = g.First().NameAr,
                NameEn = g.First().NameEn
            })
            .OrderBy(c => c.Code)
            .ToList();
    }

    /// <summary>
    /// UC-CHQ-08 — a bank's cheque stationery print offsets. Configured is true only
    /// when every offset is stored; otherwise the caller falls back to the default layout.
    /// </summary>
    public async Task<BankChequePositionsDto> GetBankChequePositionsAsync(int bankId)
    {
        var bank = await _bankRepository.AsQueryable().FirstOrDefaultAsync(b => b.Id == bankId);
        if (bank is null)
        {
            throw new KeyNotFoundException($"Bank {bankId} not found");
        }

        return new BankChequePositionsDto
        {
            BankId = bank.Id,
            BankName = bank.Name,
            Configured = bank.ChequeDateX.HasValue && bank.ChequeDateY.HasValue
                && bank.PayeeX.HasValue && bank.PayeeY.HasValue
                && bank.AmountX.HasValue && bank.AmountY.HasValue
                && bank.AmountWordsX.HasValue && bank.AmountWordsY.HasValue,
            DateX = bank.ChequeDateX,
            DateY = bank.ChequeDateY,
            PayeeX = bank.PayeeX,
            PayeeY = bank.PayeeY,
            AmountX = bank.AmountX,
            AmountY = bank.AmountY,
            AmountWordsX = bank.AmountWordsX,
            AmountWordsY = bank.AmountWordsY
        };
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
                "OfficeProjectTypes" => await ExportTableAsync(_officeProjectTypeRepository, exportDto),
                "HousingBuildings" => await ExportTableAsync(_housingBuildingRepository, exportDto),
                "HousingFlats" => await ExportTableAsync(_housingFlatRepository, exportDto),
                "OutgoingCategories" => await ExportTableAsync(_outgoingCategoryRepository, exportDto),
                "HouseOwnerships" => await ExportTableAsync(_houseOwnershipRepository, exportDto),
                "IncomeTypes" => await ExportTableAsync(_incomeTypeRepository, exportDto),
                "FamilyProjectStatuses" => await ExportTableAsync(_familyProjectStatusRepository, exportDto),
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