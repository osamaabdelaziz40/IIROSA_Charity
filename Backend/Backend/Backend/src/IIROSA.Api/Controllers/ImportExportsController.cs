using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.ImportExport;

namespace IIROSA.Api.Controllers;

/// <summary>
/// Import/Export Controller
/// Implements all import/export management endpoints (UC-12.1 through UC-12.14)
/// IMPORTANT: Charity users CANNOT access this module
/// Available to: Admin, Super Admin, Accountant, Employee
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin,Accountant,Employee")]
public class ImportExportsController : ControllerBase
{
    private readonly IImportExportService _importExportService;
    private readonly ILogger<ImportExportsController> _logger;

    public ImportExportsController(
        IImportExportService importExportService,
        ILogger<ImportExportsController> logger)
    {
        _importExportService = importExportService;
        _logger = logger;
    }

    #region Import Operations (UC-12.1, UC-12.2)

    /// <summary>
    /// Upload file for import (UC-12.1, UC-12.2)
    /// </summary>
    [HttpPost("import/upload")]
    [ProducesResponseType(typeof(ImportExportLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ImportExportLogDto>> UploadFile([FromForm] FileUploadDto dto)
    {
        try
        {
            if (dto.File == null || dto.File.Length == 0)
            {
                return BadRequest(new { message = "File is required" });
            }

            var log = await _importExportService.UploadFileAsync(dto);
            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
        }
    }

    /// <summary>
    /// Validate import data (UC-12.3)
    /// </summary>
    [HttpPost("import/{id}/validate")]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ValidationResultDto>> ValidateImport(Guid id)
    {
        try
        {
            var result = await _importExportService.ValidateImportAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating import: {Id}", id);
            return StatusCode(500, new { message = "Error validating import", error = ex.Message });
        }
    }

    /// <summary>
    /// Preview import before commit (UC-12.5)
    /// </summary>
    [HttpPost("import/{id}/preview")]
    [ProducesResponseType(typeof(ImportPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportPreviewDto>> PreviewImport(Guid id)
    {
        try
        {
            var preview = await _importExportService.PreviewImportAsync(id);
            return Ok(preview);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error previewing import: {Id}", id);
            return StatusCode(500, new { message = "Error previewing import", error = ex.Message });
        }
    }

    /// <summary>
    /// Commit import (UC-12.6)
    /// </summary>
    [HttpPost("import/commit")]
    [ProducesResponseType(typeof(ImportExportLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportExportLogDto>> CommitImport([FromBody] ImportCommitDto dto)
    {
        try
        {
            var log = await _importExportService.CommitImportAsync(dto);
            return Ok(log);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing import");
            return StatusCode(500, new { message = "Error committing import", error = ex.Message });
        }
    }

    #endregion

    #region Rollback (UC-12.8)

    /// <summary>
    /// Rollback import (UC-12.8) - Admin/SuperAdmin only
    /// </summary>
    [HttpPost("import/rollback")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(typeof(ImportExportLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportExportLogDto>> RollbackImport([FromBody] RollbackImportDto dto)
    {
        try
        {
            var log = await _importExportService.RollbackImportAsync(dto);
            return Ok(log);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rolling back import");
            return StatusCode(500, new { message = "Error rolling back import", error = ex.Message });
        }
    }

    #endregion

    #region Import History (UC-12.7)

    /// <summary>
    /// Get import history (UC-12.7)
    /// </summary>
    [HttpGet("import/history")]
    [ProducesResponseType(typeof(IEnumerable<ImportExportLogListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<ImportExportLogListDto> Items, int TotalCount)>> GetImportHistory(
        [FromQuery] ImportExportLogFilterDto filter)
    {
        try
        {
            var result = await _importExportService.GetImportHistoryAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving import history");
            return StatusCode(500, new { message = "Error retrieving import history", error = ex.Message });
        }
    }

    #endregion

    #region Export Operations (UC-12.10, UC-12.11)

    /// <summary>
    /// Export correspondence (UC-12.10, UC-12.11)
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(ImportExportLogDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportExportLogDto>> ExportCorrespondence([FromBody] ExportRequestDto dto)
    {
        try
        {
            var log = await _importExportService.ExportCorrespondenceAsync(dto);
            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting correspondence");
            return StatusCode(500, new { message = "Error exporting correspondence", error = ex.Message });
        }
    }

    /// <summary>
    /// Generate and download export file
    /// </summary>
    [HttpPost("export/generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GenerateExportFile([FromBody] ExportRequestDto dto)
    {
        try
        {
            var fileBytes = await _importExportService.GenerateExportFileAsync(dto);

            string contentType = dto.ExportFormat.ToLower() switch
            {
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "csv" => "text/csv",
                "pdf" => "application/pdf",
                _ => "application/octet-stream"
            };

            string fileExtension = dto.ExportFormat.ToLower() switch
            {
                "excel" => "xlsx",
                "csv" => "csv",
                "pdf" => "pdf",
                _ => "dat"
            };

            return File(fileBytes, contentType, $"{dto.CorrespondenceType}_Export_{DateTime.UtcNow:yyyyMMdd}.{fileExtension}");
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "File generation not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating export file");
            return StatusCode(500, new { message = "Error generating export file", error = ex.Message });
        }
    }

    #endregion

    #region Export History (UC-12.14)

    /// <summary>
    /// Get export history (UC-12.14)
    /// </summary>
    [HttpGet("export/history")]
    [ProducesResponseType(typeof(IEnumerable<ImportExportLogListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<(IEnumerable<ImportExportLogListDto> Items, int TotalCount)>> GetExportHistory(
        [FromQuery] ImportExportLogFilterDto filter)
    {
        try
        {
            var result = await _importExportService.GetExportHistoryAsync(filter);
            return Ok(new { result.Items, result.TotalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving export history");
            return StatusCode(500, new { message = "Error retrieving export history", error = ex.Message });
        }
    }

    #endregion

    #region Templates (UC-12.9)

    /// <summary>
    /// Download import template (UC-12.9)
    /// </summary>
    [HttpGet("import/template/{correspondenceType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> DownloadImportTemplate(string correspondenceType)
    {
        try
        {
            if (correspondenceType != "Incoming" && correspondenceType != "Outgoing")
            {
                return BadRequest(new { message = "Correspondence type must be 'Incoming' or 'Outgoing'" });
            }

            var templateBytes = await _importExportService.DownloadImportTemplateAsync(correspondenceType);

            return File(templateBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{correspondenceType}_Import_Template.xlsx");
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "Template generation not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading template");
            return StatusCode(500, new { message = "Error downloading template", error = ex.Message });
        }
    }

    #endregion

    #region Shared Operations

    /// <summary>
    /// Get log by ID
    /// </summary>
    [HttpGet("logs/{id}")]
    [ProducesResponseType(typeof(ImportExportLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ImportExportLogDto>> GetLogById(Guid id)
    {
        try
        {
            var log = await _importExportService.GetLogByIdAsync(id);
            if (log == null)
            {
                return NotFound(new { message = $"Log with ID '{id}' not found" });
            }

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving log: {Id}", id);
            return StatusCode(500, new { message = "Error retrieving log", error = ex.Message });
        }
    }

    /// <summary>
    /// Download exported/imported file
    /// </summary>
    [HttpGet("logs/{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DownloadLogFile(Guid id)
    {
        try
        {
            var fileBytes = await _importExportService.DownloadLogFileAsync(id);
            return File(fileBytes, "application/octet-stream", $"export_{id}.dat");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (NotImplementedException)
        {
            return NotFound(new { message = "File download not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {Id}", id);
            return StatusCode(500, new { message = "Error downloading file", error = ex.Message });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get import/export statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ImportExportStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ImportExportStatisticsDto>> GetStatistics()
    {
        try
        {
            var stats = await _importExportService.GetStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, new { message = "Error retrieving statistics", error = ex.Message });
        }
    }

    #endregion
}
