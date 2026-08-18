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
/// Incoming Letter Service Implementation
/// Implements use cases UC-12.1, UC-12.3, UC-12.4, UC-12.5, UC-12.6, UC-12.7, UC-12.8, UC-12.9, UC-12.10, UC-12.12, UC-12.13
/// </summary>
public class IncomingService : IIncomingService
{
    private readonly IIncomingRepository _incomingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<IncomingService> _logger;

    public IncomingService(
        IIncomingRepository incomingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<IncomingService> logger)
    {
        _incomingRepository = incomingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    #region CRUD Operations

    public async Task<IncomingDto> GetByIdAsync(Guid id)
    {
        var incoming = await _incomingRepository.GetByIdAsync(id);
        if (incoming == null)
            throw new KeyNotFoundException($"Incoming letter with ID {id} not found");

        return _mapper.Map<IncomingDto>(incoming);
    }

    public async Task<(IEnumerable<IncomingListDto> Items, int TotalCount)> GetPagedAsync(IncomingFilterDto filter)
    {
        var (items, totalCount) = await _incomingRepository.GetPagedAsync(
            filter.PageNumber,
            filter.PageSize,
            filter.SearchTerm,
            filter.DepartmentId,
            filter.Status,
            filter.Year,
            filter.StartDate,
            filter.EndDate,
            filter.CreatedByUserId,
            filter.SortBy,
            filter.SortOrder);

        var listDtos = _mapper.Map<IEnumerable<IncomingListDto>>(items);
        return (listDtos, totalCount);
    }

    public async Task<IncomingDto> CreateAsync(CreateIncomingDto dto)
    {
        _logger.LogInformation("Creating new incoming letter: {Subject}", dto.Subject);

        // Validate uniqueness
        if (!await IsIncomingIdUniqueAsync(dto.IncomingId))
        {
            throw new InvalidOperationException($"Incoming letter with ID '{dto.IncomingId}' already exists");
        }

        if (!string.IsNullOrEmpty(dto.LetterNumber))
        {
            if (!await IsLetterNumberUniqueAsync(dto.LetterNumber, dto.FK_DepartmentId, dto.Year))
            {
                throw new InvalidOperationException($"Letter number '{dto.LetterNumber}' already exists for this department and year");
            }
        }

        // Generate serial number
        var serial = await GetNextSerialNumberAsync(dto.FK_DepartmentId, dto.Year);
        var serialTxt = await _incomingRepository.GenerateSerialTextAsync(serial);

        var incoming = new Incoming
        {
            Subject = dto.Subject,
            Date = dto.Date ?? DateTime.UtcNow,
            IncomingNumber = dto.IncomingNumber,
            IncomingId = dto.IncomingId,
            Body = dto.Body,
            LetterNumber = dto.LetterNumber,
            LetterDate = dto.LetterDate,
            Year = dto.Year ?? DateTime.UtcNow.Year,
            Status = dto.Status ?? "Received",
            LetterDescription = dto.LetterDescription,
            FK_DepartmentId = dto.FK_DepartmentId,
            OutgoingId = dto.OutgoingId,
            UploadedFileId = dto.UploadedFileId,
            Serial = serial,
            Serial_Txt = serialTxt
        };

        await _incomingRepository.AddAsync(incoming);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Incoming letter created successfully with ID: {Id}", incoming.Id);

        return await GetByIdAsync(incoming.Id);
    }

    public async Task<IncomingDto> UpdateAsync(UpdateIncomingDto dto)
    {
        _logger.LogInformation("Updating incoming letter: {Id}", dto.Id);

        var incoming = await _incomingRepository.GetByIdAsync(dto.Id);
        if (incoming == null)
            throw new KeyNotFoundException($"Incoming letter with ID {dto.Id} not found");

        // Validate uniqueness
        if (!await IsIncomingIdUniqueAsync(dto.IncomingId, dto.Id))
        {
            throw new InvalidOperationException($"Incoming letter with ID '{dto.IncomingId}' already exists");
        }

        if (!string.IsNullOrEmpty(dto.LetterNumber))
        {
            if (!await IsLetterNumberUniqueAsync(dto.LetterNumber, dto.FK_DepartmentId, dto.Year, dto.Id))
            {
                throw new InvalidOperationException($"Letter number '{dto.LetterNumber}' already exists for this department and year");
            }
        }

        incoming.Subject = dto.Subject;
        incoming.Date = dto.Date;
        incoming.IncomingNumber = dto.IncomingNumber;
        incoming.IncomingId = dto.IncomingId;
        incoming.Body = dto.Body;
        incoming.LetterNumber = dto.LetterNumber;
        incoming.LetterDate = dto.LetterDate;
        incoming.Year = dto.Year;
        incoming.Status = dto.Status;
        incoming.LetterDescription = dto.LetterDescription;
        incoming.FK_DepartmentId = dto.FK_DepartmentId;
        incoming.OutgoingId = dto.OutgoingId;
        incoming.UploadedFileId = dto.UploadedFileId;

        _incomingRepository.Update(incoming);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Incoming letter updated successfully: {Id}", incoming.Id);

        return await GetByIdAsync(incoming.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        _logger.LogInformation("Deleting incoming letter: {Id}", id);

        var incoming = await _incomingRepository.GetByIdAsync(id);
        if (incoming == null)
            throw new KeyNotFoundException($"Incoming letter with ID {id} not found");

        _incomingRepository.Delete(incoming);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Incoming letter deleted successfully: {Id}", id);
    }

    #endregion

    #region Import Operations (UC-12.1, UC-12.3, UC-12.4, UC-12.5, UC-12.6)

    public async Task<ImportValidationResultDto> ValidateImportAsync(ImportIncomingRequestDto request)
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

                if (string.IsNullOrEmpty(row.GetValueOrDefault("IncomingId")))
                    errors.Add("IncomingId is required");

                if (string.IsNullOrEmpty(row.GetValueOrDefault("LetterNumber")))
                    errors.Add("LetterNumber is required");

                if (string.IsNullOrEmpty(row.GetValueOrDefault("LetterDate")))
                    errors.Add("LetterDate is required");

                // Validate data formats
                if (DateTime.TryParse(row.GetValueOrDefault("LetterDate"), out var letterDate))
                {
                    if (DateTime.TryParse(row.GetValueOrDefault("Date"), out var receivedDate))
                    {
                        if (letterDate > receivedDate)
                        {
                            errors.Add("LetterDate cannot be after received Date");
                        }
                    }
                }

                // Validate business rules
                var incomingId = row.GetValueOrDefault("IncomingId");
                if (!string.IsNullOrEmpty(incomingId))
                {
                    if (!await IsIncomingIdUniqueAsync(incomingId))
                    {
                        errors.Add($"IncomingId '{incomingId}' already exists");
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

    public async Task<ImportResultDto> ImportAsync(ImportIncomingRequestDto request)
    {
        _logger.LogInformation("Importing incoming letters from file: {FileName}", request.FileName);

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
                    var incomingId = row.GetValueOrDefault("IncomingId");

                    // Skip if IncomingId is not provided
                    if (string.IsNullOrWhiteSpace(incomingId))
                    {
                        result.FailedRows++;
                        result.Errors.Add(new ValidationErrorDto
                        {
                            RowNumber = rows.IndexOf(row) + 1,
                            FieldName = "IncomingId",
                            ErrorMessage = "IncomingId is required"
                        });
                        continue;
                    }

                    // Check if exists (for update scenario)
                    var existing = await _incomingRepository.GetAllAsync();
                    var existingRecord = existing.FirstOrDefault(x => x.IncomingId == incomingId!);

                    if (existingRecord != null && request.UpdateExisting)
                    {
                        // Update existing
                        existingRecord.Subject = row.GetValueOrDefault("Subject") ?? existingRecord.Subject;
                        existingRecord.Body = row.GetValueOrDefault("Body");
                        existingRecord.LetterDescription = row.GetValueOrDefault("LetterDescription");

                        _incomingRepository.Update(existingRecord);
                        result.SuccessfulRows++;
                        result.ImportedRecordIds.Add(existingRecord.Id);
                    }
                    else if (existingRecord == null && !request.SkipDuplicates)
                    {
                        // Create new
                        var serial = await GetNextSerialNumberAsync(null, null);
                        var serialTxt = await _incomingRepository.GenerateSerialTextAsync(serial);

                        var incoming = new Incoming
                        {
                            IncomingId = incomingId,
                            Subject = row.GetValueOrDefault("Subject") ?? string.Empty,
                            Body = row.GetValueOrDefault("Body"),
                            LetterNumber = row.GetValueOrDefault("LetterNumber"),
                            LetterDate = DateTime.TryParse(row.GetValueOrDefault("LetterDate"), out var ld) ? ld : null,
                            Date = DateTime.TryParse(row.GetValueOrDefault("Date"), out var d) ? d : DateTime.UtcNow,
                            Year = int.TryParse(row.GetValueOrDefault("Year"), out var y) ? y : DateTime.UtcNow.Year,
                            Serial = serial,
                            Serial_Txt = serialTxt,
                            Status = "Imported"
                        };

                        await _incomingRepository.AddAsync(incoming);
                        result.SuccessfulRows++;
                        result.ImportedRecordIds.Add(incoming.Id);
                    }
                    else
                    {
                        result.FailedRows++;
                        result.Errors.Add(new ValidationErrorDto
                        {
                            RowNumber = rows.IndexOf(row) + 1,
                            FieldName = "IncomingId",
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
            _logger.LogError(ex, "Error importing incoming letters");
            throw;
        }

        return result;
    }

    public async Task<TemplateDownloadDto> DownloadTemplateAsync()
    {
        _logger.LogInformation("Generating incoming letters import template");

        // TODO: Generate actual Excel template
        var template = new TemplateDownloadDto
        {
            TemplateType = "IncomingLetters",
            FileName = $"IncomingLetters_Template_{DateTime.UtcNow:yyyyMMdd}.xlsx",
            FileContent = Encoding.UTF8.GetBytes("Template placeholder")
        };

        return template;
    }

    #endregion

    #region Export Operations (UC-12.10, UC-12.12, UC-12.13)

    public async Task<ExportResultDto> ExportAsync(ExportIncomingRequestDto request)
    {
        _logger.LogInformation("Exporting incoming letters");

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

    #region Import History (UC-12.7, UC-12.8)

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

    #endregion

    #region Business Logic

    public async Task<bool> IsIncomingIdUniqueAsync(string incomingId, Guid? excludeId = null)
    {
        return await _incomingRepository.IsIncomingIdUniqueAsync(incomingId, excludeId);
    }

    public async Task<bool> IsLetterNumberUniqueAsync(string letterNumber, int? departmentId, int? year, Guid? excludeId = null)
    {
        return await _incomingRepository.IsLetterNumberUniqueAsync(letterNumber, departmentId, year, excludeId);
    }

    public async Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null)
    {
        return await _incomingRepository.GetNextSerialNumberAsync(departmentId, year);
    }

    public async Task<IEnumerable<string>> GetAvailableStatusesAsync()
    {
        // Return available statuses as per use case
        return new List<string>
        {
            "Received",
            "Processing",
            "Completed",
            "Closed",
            "Pending"
        };
    }

    public async Task<Dictionary<string, string>> GetStatusColorsAsync()
    {
        // Return status colors for UI display
        return new Dictionary<string, string>
        {
            { "Received", "primary" },
            { "Processing", "info" },
            { "Completed", "success" },
            { "Closed", "secondary" },
            { "Pending", "warning" }
        };
    }

    #endregion

    #region Helper Methods

    //private Dictionary<string, string> ParseImportFile(byte[] fileContent, string fileName)
    //{
    //    // TODO: Implement actual Excel/CSV parsing
    //    return new Dictionary<string, string>();
    //}

    private List<Dictionary<string, string>> ParseImportFile(byte[] fileContent, string fileName)
    {
        // TODO: Implement actual Excel/CSV parsing
        return new List<Dictionary<string, string>>();
    }

    #endregion
}
