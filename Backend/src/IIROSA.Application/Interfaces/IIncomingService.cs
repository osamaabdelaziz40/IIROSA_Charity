using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Incoming Letter Service Interface
/// Implements use cases UC-12.1, UC-12.3, UC-12.4, UC-12.5, UC-12.6, UC-12.7, UC-12.8, UC-12.9, UC-12.10, UC-12.12, UC-12.13
/// </summary>
public interface IIncomingService
{
    // CRUD Operations
    Task<IncomingDto> GetByIdAsync(Guid id);
    Task<(IEnumerable<IncomingListDto> Items, int TotalCount)> GetPagedAsync(IncomingFilterDto filter);
    Task<IncomingDto> CreateAsync(CreateIncomingDto dto);
    Task<IncomingDto> UpdateAsync(UpdateIncomingDto dto);
    Task DeleteAsync(Guid id);

    // Import Operations (UC-12.1, UC-12.3, UC-12.4, UC-12.5, UC-12.6)
    Task<ImportValidationResultDto> ValidateImportAsync(ImportIncomingRequestDto request);
    Task<ImportResultDto> ImportAsync(ImportIncomingRequestDto request);
    Task<TemplateDownloadDto> DownloadTemplateAsync();

    // Export Operations (UC-12.10, UC-12.12, UC-12.13)
    Task<ExportResultDto> ExportAsync(ExportIncomingRequestDto request);

    // Import History (UC-12.7, UC-12.8)
    Task<IEnumerable<ImportHistoryItemDto>> GetImportHistoryAsync();
    Task RollbackImportAsync(Guid importId);

    // Business Logic
    Task<bool> IsIncomingIdUniqueAsync(string incomingId, Guid? excludeId = null);
    Task<bool> IsLetterNumberUniqueAsync(string letterNumber, int? departmentId, int? year, Guid? excludeId = null);
    Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null);

    // Additional helper methods
    Task<IEnumerable<string>> GetAvailableStatusesAsync();
    Task<Dictionary<string, string>> GetStatusColorsAsync();
}
