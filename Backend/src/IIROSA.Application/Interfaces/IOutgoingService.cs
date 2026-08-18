using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Outgoing Letter Service Interface
/// Implements use cases UC-12.2, UC-12.3, UC-12.4, UC-12.5, UC-12.6, UC-12.7, UC-12.8, UC-12.11, UC-12.12, UC-12.13
/// </summary>
public interface IOutgoingService
{
    // CRUD Operations
    Task<OutgoingDto> GetByIdAsync(Guid id);
    Task<(IEnumerable<OutgoingListDto> Items, int TotalCount)> GetPagedAsync(OutgoingFilterDto filter);
    Task<OutgoingDto> CreateAsync(CreateOutgoingDto dto);
    Task<OutgoingDto> UpdateAsync(UpdateOutgoingDto dto);
    Task DeleteAsync(Guid id);

    // Import Operations (UC-12.2, UC-12.3, UC-12.4, UC-12.5, UC-12.6)
    Task<ImportValidationResultDto> ValidateImportAsync(ImportOutgoingRequestDto request);
    Task<ImportResultDto> ImportAsync(ImportOutgoingRequestDto request);
    Task<TemplateDownloadDto> DownloadTemplateAsync();

    // Export Operations (UC-12.11, UC-12.12, UC-12.13)
    Task<ExportResultDto> ExportAsync(ExportOutgoingRequestDto request);

    // Import History (UC-12.7, UC-12.8)
    Task<IEnumerable<ImportHistoryItemDto>> GetImportHistoryAsync();
    Task RollbackImportAsync(Guid importId);

    // Export History (UC-12.14)
    Task<IEnumerable<ExportHistoryItemDto>> GetExportHistoryAsync();

    // Business Logic
    Task<bool> IsOutgoingIdUniqueAsync(string outgoingId, Guid? excludeId = null);
    Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null);

    // Additional helper methods
    Task<Dictionary<int, string>> GetAvailableCategoriesAsync();
    Task<OutgoingDto> CreateChildOutgoingAsync(Guid parentOutgoingId, CreateChildOutgoingDto dto);
    Task<IEnumerable<ChildOutGoingDto>> GetChildOutgoingsAsync(Guid parentOutgoingId);
    Task<bool> DeleteChildOutgoingAsync(Guid childId);
}
