using IIROSA.Application.DTOs.ImportExport;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Import/Export Service Interface
/// Implements all use cases UC-12.1 through UC-12.14
/// </summary>
public interface IImportExportService
{
    // UC-12.1, UC-12.2: Import Incoming/Outgoing Letters
    Task<ImportExportLogDto> UploadFileAsync(FileUploadDto dto);
    Task<ValidationResultDto> ValidateImportAsync(Guid importLogId);
    Task<ImportPreviewDto> PreviewImportAsync(Guid importLogId);
    Task<ImportExportLogDto> CommitImportAsync(ImportCommitDto dto);

    // UC-12.3: Validate Import Data (included in ValidateImportAsync)

    // UC-12.4: Map Import Fields (handled in frontend, stored in FieldMapping)

    // UC-12.5: Preview Import (PreviewImportAsync)

    // UC-12.6: Commit Import (CommitImportAsync)

    // UC-12.7: View Import History
    Task<(IEnumerable<ImportExportLogListDto> Items, int TotalCount)> GetImportHistoryAsync(ImportExportLogFilterDto filter);

    // UC-12.8: Rollback Import
    Task<ImportExportLogDto> RollbackImportAsync(RollbackImportDto dto);

    // UC-12.9: Download Import Template
    Task<byte[]> DownloadImportTemplateAsync(string correspondenceType);

    // UC-12.10, UC-12.11: Export Incoming/Outgoing Letters
    Task<ImportExportLogDto> ExportCorrespondenceAsync(ExportRequestDto dto);
    Task<byte[]> GenerateExportFileAsync(ExportRequestDto dto);

    // UC-12.12: Select Export Fields (handled in ExportRequestDto)

    // UC-12.13: Filter Export Data (handled in ExportRequestDto)

    // UC-12.14: View Export History
    Task<(IEnumerable<ImportExportLogListDto> Items, int TotalCount)> GetExportHistoryAsync(ImportExportLogFilterDto filter);

    // Additional helper methods
    Task<ImportExportLogDto?> GetLogByIdAsync(Guid id);
    Task<byte[]> DownloadLogFileAsync(Guid logId);
    Task<ImportExportLogDto?> GetLatestImportAsync(string correspondenceType);
    Task<ImportExportLogDto?> GetLatestExportAsync(string correspondenceType);

    // Statistics
    Task<ImportExportStatisticsDto> GetStatisticsAsync();
}
