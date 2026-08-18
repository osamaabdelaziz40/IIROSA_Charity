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
/// Only accessible by Super Admin users
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "SuperAdminOnly", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class LookupManagementController : ControllerBase
{
    private readonly ICountryService _countryService;
    private readonly IRegionService _regionService;
    private readonly ICenterService _centerService;
    private readonly IDepartmentService _departmentService;
    private readonly IMissionTypeService _missionTypeService;
    private readonly IProjectTypeService _projectTypeService;
    private readonly IOfficeProjectTypeService _officeProjectTypeService;
    private readonly IBankService _bankService;
    private readonly INGOTypeService _ngoTypeService;
    private readonly ILookupManagementService _lookupManagementService;
    private readonly ILogger<LookupManagementController> _logger;

    public LookupManagementController(
        ICountryService countryService,
        IRegionService regionService,
        ICenterService centerService,
        IDepartmentService departmentService,
        IMissionTypeService missionTypeService,
        IProjectTypeService projectTypeService,
        IOfficeProjectTypeService officeProjectTypeService,
        IBankService bankService,
        INGOTypeService ngoTypeService,
        ILookupManagementService lookupManagementService,
        ILogger<LookupManagementController> logger)
    {
        _countryService = countryService;
        _regionService = regionService;
        _centerService = centerService;
        _departmentService = departmentService;
        _missionTypeService = missionTypeService;
        _projectTypeService = projectTypeService;
        _officeProjectTypeService = officeProjectTypeService;
        _bankService = bankService;
        _ngoTypeService = ngoTypeService;
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
    /// </summary>
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
                PageSize = 1000 // Get all active types
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

    // ==================== OTHER LOOKUP TYPES ====================
    // Similar endpoints for MissionType, ProjectType, Bank, NGOType would follow the same pattern

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
}