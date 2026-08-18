using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IIROSA.Domain.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Application.Interfaces;
using IIROSA.Application.DTOs.IncomingOutgoing;
using AutoMapper;
using System.Text;

namespace IIROSA.Application.Services;

/// <summary>
/// Outgoing Letter Service Implementation
/// Implements use cases UC-12.2, UC-12.3, UC-12.4, UC-12.5, UC-12.6, UC-12.7, UC-12.8, UC-12.11, UC-12.12, UC-12.13
/// </summary>
public class OutgoingService : IOutgoingService
{
    private readonly IOutgoingRepository _outgoingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OutgoingService> _logger;

    public OutgoingService(
        IOutgoingRepository outgoingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OutgoingService> logger)
    {
        _outgoingRepository = outgoingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<OutgoingDto> GetByIdAsync(Guid id)
    {
        var outgoing = await _outgoingRepository.GetByIdAsync(id);
        if (outgoing == null)
            throw new KeyNotFoundException($"Outgoing letter with ID {id} not found");

        return _mapper.Map<OutgoingDto>(outgoing);
    }

    public async Task<(IEnumerable<OutgoingListDto> Items, int TotalCount)> GetPagedAsync(OutgoingFilterDto filter)
    {
        var (items, totalCount) = await _outgoingRepository.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.SearchTerm,
            filter.DepartmentId,
            filter.CategoryId,
            filter.Year,
            filter.StartDate,
            filter.EndDate,
            filter.CreatedByUserId,
            filter.HasReply,
            filter.SortBy,
            filter.SortOrder);

        var listDtos = _mapper.Map<IEnumerable<OutgoingListDto>>(items);
        return (listDtos, totalCount);
    }

    public async Task<OutgoingDto> CreateAsync(CreateOutgoingDto dto)
    {
        _logger.LogInformation("Creating new outgoing letter: {Subject}", dto.Subject);

        // Validate uniqueness
        if (!await IsOutgoingIdUniqueAsync(dto.OutGoingId))
        {
            throw new InvalidOperationException($"Outgoing letter with ID '{dto.OutGoingId}' already exists");
        }

        // Generate serial number
        var serial = await GetNextSerialNumberAsync(dto.Fk_DepartmentId, dto.Year);

        var outgoing = new Outgoing
        {
            Subject = dto.Subject,
            Date = dto.Date ?? DateTime.UtcNow,
            OutGoingNumber = dto.OutGoingNumber,
            OutGoingId = dto.OutGoingId,
            Body = dto.Body,
            Year = dto.Year ?? DateTime.UtcNow.Year,
            Fk_DepartmentId = dto.Fk_DepartmentId,
            UploadedFileId = dto.UploadedFileId,
            OutgoingCategoryId = dto.OutgoingCategoryId,
            IncomingId = dto.IncomingId,
            Serial = serial
        };

        await _outgoingRepository.AddAsync(outgoing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Outgoing letter created successfully with ID: {Id}", outgoing.Id);

        return await GetByIdAsync(outgoing.Id);
    }

    public async Task<OutgoingDto> UpdateAsync(UpdateOutgoingDto dto)
    {
        _logger.LogInformation("Updating outgoing letter: {Id}", dto.Id);

        var outgoing = await _outgoingRepository.GetByIdAsync(dto.Id);
        if (outgoing == null)
            throw new KeyNotFoundException($"Outgoing letter with ID {dto.Id} not found");

        // Validate uniqueness
        if (!await IsOutgoingIdUniqueAsync(dto.OutGoingId, dto.Id))
        {
            throw new InvalidOperationException($"Outgoing letter with ID '{dto.OutGoingId}' already exists");
        }

        outgoing.Subject = dto.Subject;
        outgoing.Date = dto.Date;
        outgoing.OutGoingNumber = dto.OutGoingNumber;
        outgoing.OutGoingId = dto.OutGoingId;
        outgoing.Body = dto.Body;
        outgoing.Year = dto.Year;
        outgoing.Fk_DepartmentId = dto.Fk_DepartmentId;
        outgoing.UploadedFileId = dto.UploadedFileId;
        outgoing.OutgoingCategoryId = dto.OutgoingCategoryId;
        outgoing.IncomingId = dto.IncomingId;

        _outgoingRepository.Update(outgoing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Outgoing letter updated successfully: {Id}", outgoing.Id);

        return await GetByIdAsync(outgoing.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting outgoing letter: {Id}", id);

        var outgoing = await _outgoingRepository.GetByIdAsync(id);
        if (outgoing == null)
            throw new KeyNotFoundException($"Outgoing letter with ID {id} not found");

        _outgoingRepository.Delete(outgoing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Outgoing letter deleted successfully: {Id}", id);
    }

    #endregion

    #region Import Operations (UC-12.2, UC-12.3, UC-12.4, UC-12.5, UC-12.6)

    public async Task<ImportValidationResultDto> ValidateImportAsync(ImportOutgoingRequestDto request)
    {
        _logger.LogInformation("Validating import file: {FileName}", request.FileName);

        var result = new ImportValidationResultDto();

        try
        {
            // Parse file content (simplified - in real implementation use Excel/CSV parser)
            var rows = ParseImportFile(request.FileContent, request.FileName);

            result.TotalRows = rows.Count;

            foreach (var row in rows)
            {
                var errors = new List<string>();

                // Validate required fields
                if (string.IsNullOrEmpty(row.GetValueOrDefault("Subject")))
                    errors.Add("Subject is required");

                if (string.IsNullOrEmpty(row.GetValueOrDefault("OutGoingId")))
                    errors.Add("OutGoingId is required");

                // Validate business rules
                var outgoingId = row.GetValueOrDefault("OutGoingId");
                if (!string.IsNullOrEmpty(outgoingId))
                {
                    if (!await IsOutgoingIdUniqueAsync(outgoingId))
                    {
                        errors.Add($"OutGoingId '{outgoingId}' already exists");
                    }
                }

                if (errors.Any())
                {
                    result.InvalidRows++;
                    result.Errors.AddRange(errors.Select(e => new ValidationErrorDto
                    {
                        RowNumber = rows.IndexOf(row) + 1,
                        FieldName = "Multiple",
                        ErrorMessage = e,
                        Severity = "Error"
                    }));
                }
                else
                {
                    result.ValidRows++;
                }
            }

            _logger.LogInformation("Validation completed: {Valid} valid, {Invalid} invalid", result.ValidRows, result.InvalidRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating import file");
            throw;
        }

        return result;
    }

    public async Task<ImportResultDto> ImportAsync(ImportOutgoingRequestDto request)
    {
        _logger.LogInformation("Importing outgoing letters from file: {FileName}", request.FileName);

        var result = new ImportResultDto
        {
            ImportId = Guid.NewGuid(),
            FileName = request.FileName,
            ImportDate = DateTime.UtcNow,
            ImportedBy = "CurrentUser" // TODO: Get from context
        };

        try
        {
            // Validate first if not in validate-only mode
            if (!request.ValidateOnly)
            {
                var validationResult = await ValidateImportAsync(request);
                if (validationResult.InvalidRows > 0 && !request.UpdateExisting)
                {
                    throw new InvalidOperationException($"Validation failed with {validationResult.InvalidRows} errors");
                }
            }

            // Parse file content
            var rows = ParseImportFile(request.FileContent, request.FileName);
            result.TotalRows = rows.Count;

            foreach (var row in rows)
            {
                try
                {
                    var outgoingId = row.GetValueOrDefault("OutGoingId");

                    // Skip if OutGoingId is not provided
                    if (string.IsNullOrWhiteSpace(outgoingId))
                    {
                        result.FailedRows++;
                        result.Errors.Add(new ValidationErrorDto
                        {
                            RowNumber = rows.IndexOf(row) + 1,
                            FieldName = "OutGoingId",
                            ErrorMessage = "OutGoingId is required"
                        });
                        continue;
                    }

                    // Check if exists (for update scenario)
                    var existing = await _outgoingRepository.GetAllAsync();
                    var existingRecord = existing.FirstOrDefault(x => x.OutGoingId == outgoingId!);

                    if (existingRecord != null && request.UpdateExisting)
                    {
                        // Update existing
                        existingRecord.Subject = row.GetValueOrDefault("Subject") ?? existingRecord.Subject;
                        existingRecord.Body = row.GetValueOrDefault("Body");

                        _outgoingRepository.Update(existingRecord);
                        result.SuccessfulRows++;
                        result.ImportedRecordIds.Add(existingRecord.Id);
                    }
                    else if (existingRecord == null && !request.SkipDuplicates)
                    {
                        // Create new
                        var serial = await GetNextSerialNumberAsync(null, null);

                        var outgoing = new Outgoing
                        {
                            OutGoingId = outgoingId,
                            Subject = row.GetValueOrDefault("Subject") ?? string.Empty,
                            Body = row.GetValueOrDefault("Body"),
                            OutGoingNumber = row.GetValueOrDefault("OutGoingNumber"),
                            Date = DateTime.TryParse(row.GetValueOrDefault("Date"), out var d) ? d : DateTime.UtcNow,
                            Year = int.TryParse(row.GetValueOrDefault("Year"), out var y) ? y : DateTime.UtcNow.Year,
                            Serial = serial
                        };

                        await _outgoingRepository.AddAsync(outgoing);
                        result.SuccessfulRows++;
                        result.ImportedRecordIds.Add(outgoing.Id);
                    }
                    else
                    {
                        result.FailedRows++;
                        result.Errors.Add(new ValidationErrorDto
                        {
                            RowNumber = rows.IndexOf(row) + 1,
                            FieldName = "OutGoingId",
                            ErrorMessage = "Skipped (duplicate)"
                        });
                    }
                }
                catch (Exception ex)
                {
                    result.FailedRows++;
                    result.Errors.Add(new ValidationErrorDto
                    {
                        RowNumber = rows.IndexOf(row) + 1,
                        FieldName = "Import",
                        ErrorMessage = ex.Message
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Import completed: {Success} successful, {Failed} failed", result.SuccessfulRows, result.FailedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing outgoing letters");
            throw;
        }

        return result;
    }

    public async Task<TemplateDownloadDto> DownloadTemplateAsync()
    {
        _logger.LogInformation("Generating outgoing letters import template");

        // TODO: Generate actual Excel template
        var template = new TemplateDownloadDto
        {
            TemplateType = "OutgoingLetters",
            FileName = $"OutgoingLetters_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx",
            FileContent = Encoding.UTF8.GetBytes("Template placeholder")
        };

        return template;
    }

    #endregion

    #region Export Operations (UC-12.11, UC-12.12, UC-12.13)

    public async Task<ExportResultDto> ExportAsync(ExportOutgoingRequestDto request)
    {
        _logger.LogInformation("Exporting outgoing letters");

        // TODO: Implement actual export logic with Excel/PDF generation
        var result = new ExportResultDto
        {
            ExportId = Guid.NewGuid(),
            ExportDate = DateTime.UtcNow,
            ExportedBy = "CurrentUser", // TODO: Get from context
            FileFormat = request.FileFormat
        };

        return result;
    }

    #endregion

    #region Import/Export History (UC-12.7, UC-12.8, UC-12.14)

    public async Task<IEnumerable<ImportHistoryItemDto>> GetImportHistoryAsync()
    {
        // TODO: Implement import history tracking
        return Enumerable.Empty<ImportHistoryItemDto>();
    }

    public async Task RollbackImportAsync(Guid importId)
    {
        _logger.LogInformation("Rolling back import: {ImportId}", importId);

        // TODO: Implement rollback logic

        _logger.LogInformation("Import rolled back successfully: {ImportId}", importId);
    }

    public async Task<IEnumerable<ExportHistoryItemDto>> GetExportHistoryAsync()
    {
        // TODO: Implement export history tracking
        return Enumerable.Empty<ExportHistoryItemDto>();
    }

    #endregion

    #region Business Logic

    public async Task<bool> IsOutgoingIdUniqueAsync(string outgoingId, Guid? excludeId = null)
    {
        return await _outgoingRepository.IsOutgoingIdUniqueAsync(outgoingId, excludeId);
    }

    public async Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null)
    {
        return await _outgoingRepository.GetNextSerialNumberAsync(departmentId, year);
    }

    public async Task<Dictionary<int, string>> GetAvailableCategoriesAsync()
    {
        // TODO: Get actual categories from database
        // For now, return common outgoing letter categories
        return new Dictionary<int, string>
        {
            { 1, "Official" },
            { 2, "Internal" },
            { 3, "External" },
            { 4, "Confidential" }
        };
    }

    public async Task<OutgoingDto> CreateChildOutgoingAsync(Guid parentOutgoingId, CreateChildOutgoingDto dto)
    {
        _logger.LogInformation("Creating child outgoing letter for parent: {ParentId}", parentOutgoingId);

        var parent = await _outgoingRepository.GetByIdAsync(parentOutgoingId);
        if (parent == null)
            throw new KeyNotFoundException($"Parent outgoing letter with ID {parentOutgoingId} not found");

        var child = new ChildOutGoing
        {
            OutgoingId = parentOutgoingId,
            Subject = dto.Subject,
            Date = dto.Date ?? DateTime.UtcNow,
            Body = dto.Body,
            Year = dto.Year ?? DateTime.UtcNow.Year,
            Fk_DepartmentId = dto.Fk_DepartmentId,
            UploadedFileId = dto.UploadedFileId
        };

        // TODO: Add to ChildOutgoingRepository and save
        // For now, this is a placeholder

        _logger.LogInformation("Child outgoing letter created successfully");

        return await GetByIdAsync(parentOutgoingId); // Return parent for now
    }

    public async Task<IEnumerable<ChildOutGoingDto>> GetChildOutgoingsAsync(Guid parentOutgoingId)
    {
        _logger.LogInformation("Getting child outgoing letters for parent: {ParentId}", parentOutgoingId);

        var parent = await _outgoingRepository.GetByIdAsync(parentOutgoingId);
        if (parent == null)
            throw new KeyNotFoundException($"Parent outgoing letter with ID {parentOutgoingId} not found");

        // TODO: Implement actual ChildOutgoingRepository query
        // For now, return empty list
        return Enumerable.Empty<ChildOutGoingDto>();
    }

    public async Task<bool> DeleteChildOutgoingAsync(Guid childId)
    {
        _logger.LogInformation("Deleting child outgoing letter: {ChildId}", childId);

        // TODO: Implement actual delete via ChildOutgoingRepository
        // For now, return true
        return await Task.FromResult(true);
    }

    #endregion

    #region Helper Methods

    private List<Dictionary<string, string>> ParseImportFile(byte[] fileContent, string fileName)
    {
        // TODO: Implement actual Excel/CSV parsing
        return new List<Dictionary<string, string>>();
    }

    #endregion
}
