using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IIROSA.Application.DTOs.OfficeProjectManagement;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Lookup Management API Controller
/// Implements UC-14.1 to UC-14.15: Lookup Management operations
/// Reads are open to any authenticated user — countries/regions/centers feed the filter
/// dropdowns of ordinary list pages (charities, families, ...), not just this module.
/// Write operations are restricted to Super Admin (see the per-action policies below).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LookupManagementController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly IRegionService _regionService;
    private readonly ICenterService _centerService;
    private readonly IDepartmentService _departmentService;
    private readonly IMissionTypeService _missionTypeService;
    private readonly IMissionInterviewTypeService _missionInterviewTypeService;
    private readonly IProjectTypeService _projectTypeService;
    private readonly IOfficeProjectTypeService _officeProjectTypeService;
    private readonly IHousingBuildingService _housingBuildingService;
    private readonly IHousingFlatService _housingFlatService;
    private readonly IBankService _bankService;
    private readonly INGOTypeService _ngoTypeService;
    private readonly IEducationLevelService _educationLevelService;
    private readonly IHealthStatusService _healthStatusService;
    private readonly IRefuseReasonService _refuseReasonService;
    // Refugee register lookups (epic 7, UC-REF-03)
    private readonly IHouseOwnershipService _houseOwnershipService;
    private readonly IHouseStatusService _houseStatusService;
    private readonly IIncomeTypeService _incomeTypeService;
    private readonly ISocialStatusService _socialStatusService;
    private readonly IRelationService _relationService;
    private readonly IReasonOfRelService _reasonOfRelService;
    private readonly IHousingTypeService _housingTypeService;
    // Guardian reference data (epic 19, UC-SYS-05)
    private readonly IMaritalStatusService _maritalStatusService;
    // Guardian job / profession catalogue (epic 19, UC-SYS-09)
    private readonly IJobService _jobService;
    private readonly ILookupManagementService _lookupManagementService;
    private readonly ILogger<LookupManagementController> _logger;

    /// <summary>
    /// Dropdown "get all" page size (19-4) — equal to LookupFilterDto.MaxPageSize, so a
    /// catalogue that outgrows the ceiling is detectable on the paged endpoints (their
    /// totalCount is the true count) instead of being silently truncated here.
    /// </summary>
    private const int DropdownPageSize = LookupFilterDto.MaxPageSize;

    public LookupManagementController(
        ICountryService countryService,
        IRegionService regionService,
        ICenterService centerService,
        IDepartmentService departmentService,
        IMissionTypeService missionTypeService,
        IMissionInterviewTypeService missionInterviewTypeService,
        IProjectTypeService projectTypeService,
        IOfficeProjectTypeService officeProjectTypeService,
        IHousingBuildingService housingBuildingService,
        IHousingFlatService housingFlatService,
        IBankService bankService,
        INGOTypeService ngoTypeService,
        IEducationLevelService educationLevelService,
        IHealthStatusService healthStatusService,
        IRefuseReasonService refuseReasonService,
        IHouseOwnershipService houseOwnershipService,
        IHouseStatusService houseStatusService,
        IIncomeTypeService incomeTypeService,
        ISocialStatusService socialStatusService,
        IRelationService relationService,
        IReasonOfRelService reasonOfRelService,
        IHousingTypeService housingTypeService,
        IMaritalStatusService maritalStatusService,
        IJobService jobService,
        ILookupManagementService lookupManagementService,
        ILogger<LookupManagementController> logger)
    {
        _countryService = countryService;
        _regionService = regionService;
        _centerService = centerService;
        _departmentService = departmentService;
        _missionTypeService = missionTypeService;
        _missionInterviewTypeService = missionInterviewTypeService;
        _projectTypeService = projectTypeService;
        _officeProjectTypeService = officeProjectTypeService;
        _housingBuildingService = housingBuildingService;
        _housingFlatService = housingFlatService;
        _bankService = bankService;
        _ngoTypeService = ngoTypeService;
        _educationLevelService = educationLevelService;
        _healthStatusService = healthStatusService;
        _refuseReasonService = refuseReasonService;
        _houseOwnershipService = houseOwnershipService;
        _houseStatusService = houseStatusService;
        _incomeTypeService = incomeTypeService;
        _socialStatusService = socialStatusService;
        _relationService = relationService;
        _reasonOfRelService = reasonOfRelService;
        _housingTypeService = housingTypeService;
        _maritalStatusService = maritalStatusService;
        _jobService = jobService;
        _lookupManagementService = lookupManagementService;
        _logger = logger;
    }

    /// <summary>
    /// UC-14.5: View All Lookup Tables
    /// Get summary of all lookup tables
    /// </summary>
    [HttpGet("tables/summary")]
    public async Task<ActionResult<List<LookupTableSummaryDto>>> GetAllLookupTablesSummary()
    {
        try
        {
            var summaries = await _lookupManagementService.GetAllLookupTablesSummaryAsync();
            return Ok(summaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving lookup tables summary");
            return StatusCode(500, new { message = "An error occurred while retrieving lookup tables summary" });
        }
    }

    /// <summary>
    /// Export lookup table data (UC-14.14)
    /// 19-4: bulk table export is HQ-admin only (read-widening gate); the summary above
    /// stays readable to any authenticated role — the overview cards must keep rendering.
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpGet("tables/{tableName}/export")]
    public async Task<IActionResult> ExportLookupTable(string tableName, [FromQuery] BulkExportDto exportDto)
    {
        try
        {
            var data = await _lookupManagementService.ExportLookupTableAsync(tableName, exportDto);
            var fileName = $"{tableName}_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var contentType = "text/csv";

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while exporting lookup table: {TableName}", tableName);
            return StatusCode(500, new { message = "An error occurred while exporting lookup table" });
        }
    }

    // ==================== COUNTRIES (UC-14.8) ====================

    /// <summary>
    /// Get all countries with filtering and pagination
    /// </summary>
    [HttpGet("countries")]
    public async Task<ActionResult<LookupPagedResult<CountryDto>>> GetCountries([FromQuery] LookupFilterDto filter)
    {
        try
        {
            var result = await _countryService.GetLookupItemsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving countries");
            return StatusCode(500, new { message = "An error occurred while retrieving countries" });
        }
    }

    /// <summary>
    /// Get country by ID
    /// </summary>
    [HttpGet("countries/{id}")]
    public async Task<ActionResult<CountryDto>> GetCountry(int id)
    {
        try
        {
            var country = await _countryService.GetLookupByIdAsync(id);
            if (country == null)
            {
                return NotFound(new { message = "Country not found" });
            }

            return Ok(country);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving country {CountryId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving country" });
        }
    }

    /// <summary>
    /// UC-14.1: Create new country
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("countries")]
    public async Task<ActionResult<CountryDto>> CreateCountry([FromBody] CreateCountryDto model)
    {
        try
        {
            var country = await _countryService.CreateLookupAsync(model);
            return CreatedAtAction(nameof(GetCountry), new { id = (country as CountryDto)?.Id }, country);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating country");
            return StatusCode(500, new { message = "An error occurred while creating country" });
        }
    }

    /// <summary>
    /// UC-14.2: Update country
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("countries/{id}")]
    public async Task<ActionResult<CountryDto>> UpdateCountry(int id, [FromBody] UpdateCountryDto model)
    {
        try
        {
            var country = await _countryService.UpdateLookupAsync(id, model);
            return Ok(country);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating country {CountryId}", id);
            return StatusCode(500, new { message = "An error occurred while updating country" });
        }
    }

    /// <summary>
    /// UC-14.3: Deactivate country
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPatch("countries/{id}/deactivate")]
    public async Task<ActionResult> DeactivateCountry(int id)
    {
        try
        {
            await _countryService.DeactivateLookupAsync(id);
            _logger.LogInformation("Country {CountryId} deactivated by {DeactivatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Country deactivated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating country {CountryId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating country" });
        }
    }

    /// <summary>
    /// Activate country
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPatch("countries/{id}/activate")]
    public async Task<ActionResult> ActivateCountry(int id)
    {
        try
        {
            await _countryService.ActivateLookupAsync(id);
            _logger.LogInformation("Country {CountryId} activated by {ActivatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Country activated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating country {CountryId}", id);
            return StatusCode(500, new { message = "An error occurred while activating country" });
        }
    }

    /// <summary>
    /// UC-14.4: Set country sort order
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPatch("countries/{id}/sortorder")]
    public async Task<ActionResult> UpdateCountrySortOrder(int id, [FromBody] int sortOrder)
    {
        try
        {
            await _countryService.UpdateSortOrderAsync(id, sortOrder);
            return Ok(new { message = "Country sort order updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating sort order for country {CountryId}", id);
            return StatusCode(500, new { message = "An error occurred while updating sort order" });
        }
    }

    // ==================== REGIONS (UC-14.7) ====================

    /// <summary>
    /// Get all regions with filtering and pagination
    /// </summary>
    [HttpGet("regions")]
    public async Task<ActionResult<LookupPagedResult<RegionDto>>> GetRegions([FromQuery] LookupFilterDto filter)
    {
        try
        {
            var result = await _regionService.GetLookupItemsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving regions");
            return StatusCode(500, new { message = "An error occurred while retrieving regions" });
        }
    }

    /// <summary>
    /// Get regions by country
    /// </summary>
    [HttpGet("regions/by-country/{countryId}")]
    public async Task<ActionResult<List<RegionDto>>> GetRegionsByCountry(int countryId)
    {
        try
        {
            var regions = await _regionService.GetRegionsByCountryAsync(countryId);
            return Ok(regions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving regions for country {CountryId}", countryId);
            return StatusCode(500, new { message = "An error occurred while retrieving regions" });
        }
    }

    /// <summary>
    /// Get region by ID
    /// </summary>
    [HttpGet("regions/{id}")]
    public async Task<ActionResult<RegionDto>> GetRegion(int id)
    {
        try
        {
            var region = await _regionService.GetLookupByIdAsync(id);
            if (region == null)
            {
                return NotFound(new { message = "Region not found" });
            }

            return Ok(region);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving region {RegionId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving region" });
        }
    }

    /// <summary>
    /// Create new region (UC-14.7)
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("regions")]
    public async Task<ActionResult<RegionDto>> CreateRegion([FromBody] CreateRegionDto model)
    {
        try
        {
            var region = await _regionService.CreateLookupAsync(model);
            return CreatedAtAction(nameof(GetRegion), new { id = (region as RegionDto)?.Id }, region);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating region");
            return StatusCode(500, new { message = "An error occurred while creating region" });
        }
    }

    /// <summary>
    /// Update region (UC-14.7)
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("regions/{id}")]
    public async Task<ActionResult<RegionDto>> UpdateRegion(int id, [FromBody] UpdateRegionDto model)
    {
        try
        {
            var region = await _regionService.UpdateLookupAsync(id, model);
            return Ok(region);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating region {RegionId}", id);
            return StatusCode(500, new { message = "An error occurred while updating region" });
        }
    }

    // ==================== CENTERS (UC-14.6) ====================

    /// <summary>
    /// Get all centers with filtering and pagination
    /// </summary>
    [HttpGet("centers")]
    public async Task<ActionResult<LookupPagedResult<CenterDto>>> GetCenters([FromQuery] LookupFilterDto filter)
    {
        try
        {
            var result = await _centerService.GetLookupItemsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving centers");
            return StatusCode(500, new { message = "An error occurred while retrieving centers" });
        }
    }

    /// <summary>
    /// Get centers by region
    /// </summary>
    [HttpGet("centers/by-region/{regionId}")]
    public async Task<ActionResult<List<CenterDto>>> GetCentersByRegion(int regionId)
    {
        try
        {
            var centers = await _centerService.GetCentersByRegionAsync(regionId);
            return Ok(centers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving centers for region {RegionId}", regionId);
            return StatusCode(500, new { message = "An error occurred while retrieving centers" });
        }
    }

    /// <summary>
    /// Get center by ID
    /// </summary>
    [HttpGet("centers/{id}")]
    public async Task<ActionResult<CenterDto>> GetCenter(int id)
    {
        try
        {
            var center = await _centerService.GetLookupByIdAsync(id);
            if (center == null)
            {
                return NotFound(new { message = "Center not found" });
            }

            return Ok(center);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving center {CenterId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving center" });
        }
    }

    /// <summary>
    /// Create new center (UC-14.6)
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("centers")]
    public async Task<ActionResult<CenterDto>> CreateCenter([FromBody] CreateCenterDto model)
    {
        try
        {
            var center = await _centerService.CreateLookupAsync(model);
            return CreatedAtAction(nameof(GetCenter), new { id = (center as CenterDto)?.Id }, center);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating center");
            return StatusCode(500, new { message = "An error occurred while creating center" });
        }
    }

    /// <summary>
    /// Update center (UC-14.6)
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("centers/{id}")]
    public async Task<ActionResult<CenterDto>> UpdateCenter(int id, [FromBody] UpdateCenterDto model)
    {
        try
        {
            var center = await _centerService.UpdateLookupAsync(id, model);
            return Ok(center);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating center {CenterId}", id);
            return StatusCode(500, new { message = "An error occurred while updating center" });
        }
    }

    // ==================== DEPARTMENTS (UC-14.9) ====================

    /// <summary>
    /// Get all departments with filtering and pagination
    /// </summary>
    [HttpGet("departments")]
    public async Task<ActionResult<LookupPagedResult<DepartmentDto>>> GetDepartments([FromQuery] LookupFilterDto filter)
    {
        try
        {
            var result = await _departmentService.GetLookupItemsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving departments");
            return StatusCode(500, new { message = "An error occurred while retrieving departments" });
        }
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("departments/{id}")]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
    {
        try
        {
            var department = await _departmentService.GetLookupByIdAsync(id);
            if (department == null)
            {
                return NotFound(new { message = "Department not found" });
            }

            return Ok(department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving department" });
        }
    }

    /// <summary>
    /// Create new department (UC-14.9)
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("departments")]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto model)
    {
        try
        {
            var department = await _departmentService.CreateLookupAsync(model);
            return CreatedAtAction(nameof(GetDepartment), new { id = (department as DepartmentDto)?.Id }, department);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating department");
            return StatusCode(500, new { message = "An error occurred while creating department" });
        }
    }

    /// <summary>
    /// Update department (UC-14.9)
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("departments/{id}")]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(int id, [FromBody] UpdateDepartmentDto model)
    {
        try
        {
            var department = await _departmentService.UpdateLookupAsync(id, model);
            return Ok(department);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating department {DepartmentId}", id);
            return StatusCode(500, new { message = "An error occurred while updating department" });
        }
    }

    // ==================== BANKS ====================

    /// <summary>
    /// Get all banks with filtering and pagination
    /// </summary>
    [HttpGet("banks")]
    public async Task<ActionResult<LookupPagedResult<BankDto>>> GetBanks([FromQuery] LookupFilterDto filter)
    {
        try
        {
            var result = await _bankService.GetLookupItemsAsync(filter);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving banks");
            return StatusCode(500, new { message = "An error occurred while retrieving banks" });
        }
    }

    /// <summary>
    /// Get bank by ID
    /// </summary>
    [HttpGet("banks/{id}")]
    public async Task<ActionResult<BankDto>> GetBank(int id)
    {
        try
        {
            var bank = await _bankService.GetLookupByIdAsync(id);
            if (bank == null)
            {
                return NotFound(new { message = "Bank not found" });
            }

            return Ok(bank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving bank {BankId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving bank" });
        }
    }

    /// <summary>
    /// Create new bank
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPost("banks")]
    public async Task<ActionResult<BankDto>> CreateBank([FromBody] CreateBankDto model)
    {
        try
        {
            var bank = await _bankService.CreateLookupAsync(model);
            return CreatedAtAction(nameof(GetBank), new { id = (bank as BankDto)?.Id }, bank);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating bank");
            return StatusCode(500, new { message = "An error occurred while creating bank" });
        }
    }

    /// <summary>
    /// Update bank
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPut("banks/{id}")]
    public async Task<ActionResult<BankDto>> UpdateBank(int id, [FromBody] UpdateBankDto model)
    {
        try
        {
            var bank = await _bankService.UpdateLookupAsync(id, model);
            return Ok(bank);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating bank {BankId}", id);
            return StatusCode(500, new { message = "An error occurred while updating bank" });
        }
    }

    /// <summary>
    /// Delete bank
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpDelete("banks/{id}")]
    public async Task<ActionResult> DeleteBank(int id)
    {
        try
        {
            await _bankService.DeleteLookupAsync(id);
            _logger.LogInformation("Bank {BankId} deleted by {DeletedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Bank deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting bank {BankId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting bank" });
        }
    }

    /// <summary>
    /// Activate bank
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPatch("banks/{id}/activate")]
    public async Task<ActionResult> ActivateBank(int id)
    {
        try
        {
            await _bankService.ActivateLookupAsync(id);
            _logger.LogInformation("Bank {BankId} activated by {ActivatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Bank activated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating bank {BankId}", id);
            return StatusCode(500, new { message = "An error occurred while activating bank" });
        }
    }

    /// <summary>
    /// Deactivate bank
    /// </summary>
    [Authorize(Policy = "SuperAdminOnly")]
    [HttpPatch("banks/{id}/deactivate")]
    public async Task<ActionResult> DeactivateBank(int id)
    {
        try
        {
            await _bankService.DeactivateLookupAsync(id);
            _logger.LogInformation("Bank {BankId} deactivated by {DeactivatedBy}", id, User.Identity?.Name);
            return Ok(new { message = "Bank deactivated successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating bank {BankId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating bank" });
        }
    }

    // ==================== CHEQUE SUPPORT LOOKUPS (chapter 16, UC-CHQ) ====================

    /// <summary>
    /// UC-CHQ-05 — cheque beneficiary type-ahead for the cheque form (§16.S.2).
    /// </summary>
    [HttpGet("cheque-beneficiaries")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    public async Task<ActionResult<List<ChequeBeneficiaryOptionDto>>> GetChequeBeneficiaries(
        [FromQuery] string? term,
        [FromQuery] int take = 20)
    {
        try
        {
            return Ok(await _lookupManagementService.GetChequeBeneficiariesAsync(term, take));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cheque beneficiaries");
            return StatusCode(500, new { message = "An error occurred while retrieving cheque beneficiaries" });
        }
    }

    /// <summary>
    /// UC-CHQ-06 — the distinct currencies configured on countries, for the cheque form.
    /// </summary>
    [HttpGet("currencies")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    public async Task<ActionResult<List<CurrencyOptionDto>>> GetCurrencies()
    {
        try
        {
            return Ok(await _lookupManagementService.GetCurrenciesAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving currencies");
            return StatusCode(500, new { message = "An error occurred while retrieving currencies" });
        }
    }

    /// <summary>
    /// UC-CHQ-08 — a bank's cheque stationery print offsets (طباعة مصري alignment data).
    /// </summary>
    [HttpGet("banks/{bankId}/cheque-positions")]
    [Authorize(Roles = "SuperAdmin,Admin,Accountant,FinancialOfficer")]
    public async Task<ActionResult<BankChequePositionsDto>> GetBankChequePositions(int bankId)
    {
        try
        {
            return Ok(await _lookupManagementService.GetBankChequePositionsAsync(bankId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving cheque positions for bank {BankId}", bankId);
            return StatusCode(500, new { message = "An error occurred while retrieving cheque positions" });
        }
    }

    // ==================== OFFICE PROJECT TYPES ====================

    /// <summary>
    /// Get all active office project types for dropdown
    /// Accessible by Admin and SuperAdmin roles
    /// </summary>
    [HttpGet("office-project-types")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<List<LookupDto>>> GetOfficeProjectTypes()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = DropdownPageSize // Get all active types
            };
            var result = await _officeProjectTypeService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving office project types");
            return StatusCode(500, new { message = "An error occurred while retrieving office project types" });
        }
    }

    /// <summary>
    /// UC-HOU-05: Select building and flat — the §11.S.2 رقم العماره drop-down source.
    /// Active-only housing buildings (organisation-owned catalogue). Charity callers read
    /// this for the housing family form, so the action admits the module's full role set.
    /// </summary>
    [HttpGet("housing-buildings")]
    [Authorize(Roles = "Admin,SuperAdmin,Charity")]
    public async Task<ActionResult<List<LookupDto>>> GetHousingBuildings()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = DropdownPageSize // Get all active buildings
            };
            var result = await _housingBuildingService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving housing buildings");
            return StatusCode(500, new { message = "An error occurred while retrieving housing buildings" });
        }
    }

    /// <summary>
    /// UC-HOU-05: flats of one building — the §11.S.2 رقم الشقه cascade (republished on
    /// رقم العماره change). buildingId is required; absent or non-positive is a 400.
    /// </summary>
    [HttpGet("housing-flats")]
    [Authorize(Roles = "Admin,SuperAdmin,Charity")]
    public async Task<ActionResult<List<LookupDto>>> GetHousingFlats([FromQuery] int? buildingId)
    {
        if (!buildingId.HasValue || buildingId.Value <= 0)
        {
            return BadRequest(new { message = "buildingId is required" });
        }

        try
        {
            var flats = await _housingFlatService.GetFlatsByBuildingAsync(buildingId.Value);
            return Ok(flats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving housing flats for building {BuildingId}", buildingId.Value);
            return StatusCode(500, new { message = "An error occurred while retrieving housing flats" });
        }
    }

    // ==================== OTHER LOOKUP TYPES ====================
    // Similar endpoints for MissionType, ProjectType, Bank, NGOType would follow the same pattern

    /// <summary>
    /// Get all active mission interview types for dropdown (UC-MSN-04)
    /// Accessible by Admin and SuperAdmin roles
    /// </summary>
    [HttpGet("mission-interview-types")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<ActionResult<List<MissionInterviewTypeDto>>> GetMissionInterviewTypes()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = DropdownPageSize // Get all active types
            };
            var result = await _missionInterviewTypeService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving mission interview types");
            return StatusCode(500, new { message = "An error occurred while retrieving mission interview types" });
        }
    }

    /// <summary>
    /// Get all lookup types
    /// </summary>
    [HttpGet("types")]
    public ActionResult<object> GetLookupTypes()
    {
        var types = new[]
        {
            new { name = "Countries", arabicName = "البلدان", endpoint = "countries" },
            new { name = "Regions", arabicName = "المناطق", endpoint = "regions" },
            new { name = "Centers", arabicName = "المراكز", endpoint = "centers" },
            new { name = "Departments", arabicName = "الأقسام", endpoint = "departments" },
            new { name = "Mission Types", arabicName = "أنواع المهام", endpoint = "missiontypes" },
            new { name = "Project Types", arabicName = "أنواع المشاريع", endpoint = "projecttypes" },
            new { name = "Banks", arabicName = "البنوك", endpoint = "banks" },
            new { name = "NGO Types", arabicName = "أنواع الجمعيات", endpoint = "ngotypes" }
        };

        return Ok(types);
    }

    // ==================== ORPHAN REFERENCE DATA (UC-ORP-11) ====================

    /// <summary>
    /// Get all active education levels for the orphan-form dropdown (UC-ORP-11).
    /// Global reference data — no charity scoping; an empty list is a valid answer.
    /// </summary>
    [HttpGet("education-levels")]
    public async Task<ActionResult<List<EducationLevelDto>>> GetEducationLevels()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = DropdownPageSize // Get all active levels
            };
            var result = await _educationLevelService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving education levels");
            return StatusCode(500, new { message = "An error occurred while retrieving education levels" });
        }
    }

    /// <summary>
    /// Get all active health statuses for the orphan-form dropdown (UC-ORP-11).
    /// Global reference data — no charity scoping; an empty list is a valid answer.
    /// </summary>
    [HttpGet("health-statuses")]
    public async Task<ActionResult<List<HealthStatusDto>>> GetHealthStatuses()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = DropdownPageSize // Get all active statuses
            };
            var result = await _healthStatusService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving health statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving health statuses" });
        }
    }

    /// <summary>
    /// Get all active refuse reasons for the periodic report refusal dropdown (epic 9, UC-ORR-08).
    /// Global reference data — no charity scoping; an empty list is a valid answer.
    /// </summary>
    [HttpGet("refuse-reasons")]
    public async Task<ActionResult<List<RefuseReasonDto>>> GetRefuseReasons()
    {
        try
        {
            var filter = new LookupFilterDto
            {
                IsActive = true,
                Page = 1,
                PageSize = DropdownPageSize // Get all active reasons
            };
            var result = await _refuseReasonService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving refuse reasons");
            return StatusCode(500, new { message = "An error occurred while retrieving refuse reasons" });
        }
    }

    // ==================== REFUGEE REGISTER REFERENCE DATA (epic 7, UC-REF-03) ====================
    // Global catalogues for the refugee form's drop-downs (§12.S.2) — same shape as
    // education-levels/health-statuses above: active items, no charity scoping, empty is valid.

    /// <summary>House ownerships — ملكية السكن (§12.S.2)</summary>
    [HttpGet("house-ownerships")]
    public async Task<ActionResult<List<HouseOwnershipDto>>> GetHouseOwnerships()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _houseOwnershipService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving house ownerships");
            return StatusCode(500, new { message = "An error occurred while retrieving house ownerships" });
        }
    }

    /// <summary>House contents statuses — حالة محتويات السكن (§12.S.2)</summary>
    [HttpGet("house-statuses")]
    public async Task<ActionResult<List<HouseStatusDto>>> GetHouseStatuses()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _houseStatusService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving house statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving house statuses" });
        }
    }

    /// <summary>Income types — نوع الدخل (§12.S.2)</summary>
    [HttpGet("income-types")]
    public async Task<ActionResult<List<IncomeTypeDto>>> GetIncomeTypes()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _incomeTypeService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving income types");
            return StatusCode(500, new { message = "An error occurred while retrieving income types" });
        }
    }

    /// <summary>Social statuses — الحالة الاجتماعية of an اضافة ابن / اضافة مرافق (§12.S.2)</summary>
    [HttpGet("social-statuses")]
    public async Task<ActionResult<List<SocialStatusDto>>> GetSocialStatuses()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _socialStatusService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving social statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving social statuses" });
        }
    }

    /// <summary>Relations — نوع العلاقة of a provider, نوعها (§12.S.2)</summary>
    [HttpGet("relations")]
    public async Task<ActionResult<List<RelationDto>>> GetRelations()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _relationService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving relations");
            return StatusCode(500, new { message = "An error occurred while retrieving relations" });
        }
    }

    /// <summary>Reasons of relation — السبب of a provider's link to the family (§12.S.2)</summary>
    [HttpGet("reasons-of-relation")]
    public async Task<ActionResult<List<ReasonOfRelDto>>> GetReasonsOfRelation()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _reasonOfRelService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving reasons of relation");
            return StatusCode(500, new { message = "An error occurred while retrieving reasons of relation" });
        }
    }

    /// <summary>Housing types — نوع السكن, shared catalogue (§12.S.2 refugee form / legacy Family.HousingTypeId)</summary>
    [HttpGet("housing-types")]
    public async Task<ActionResult<List<HousingTypeDto>>> GetHousingTypes()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _housingTypeService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving housing types");
            return StatusCode(500, new { message = "An error occurred while retrieving housing types" });
        }
    }

    // ==================== GUARDIAN REFERENCE DATA (epic 19, UC-SYS-05) ====================

    /// <summary>
    /// Get all active marital statuses for the guardian (provider/parent) sections of the
    /// family forms. Global reference data — no charity scoping; an empty list is a valid answer.
    /// </summary>
    [HttpGet("marital-statuses")]
    public async Task<ActionResult<List<MaritalStatusDto>>> GetMaritalStatuses()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _maritalStatusService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving marital statuses");
            return StatusCode(500, new { message = "An error occurred while retrieving marital statuses" });
        }
    }

    /// <summary>
    /// Get all active jobs (professions) for the guardian (provider/parent) sections of the
    /// family forms. Global reference data — no charity scoping; an empty list is a valid answer.
    /// </summary>
    [HttpGet("jobs")]
    public async Task<ActionResult<List<JobDto>>> GetJobs()
    {
        try
        {
            var filter = new LookupFilterDto { IsActive = true, Page = 1, PageSize = DropdownPageSize };
            var result = await _jobService.GetLookupItemsAsync(filter);
            return Ok(result.Items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving jobs");
            return StatusCode(500, new { message = "An error occurred while retrieving jobs" });
        }
    }
}