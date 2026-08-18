using IIROSA.Application.DTOs.ImportExport;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Import/Export Repository Interface
/// Defines data access operations for ImportExportLog entity
/// </summary>
public interface IImportExportRepository
{
    // Basic CRUD
    Task<ImportExportLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<ImportExportLog>> GetAllAsync();
    Task<(IEnumerable<ImportExportLog> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task AddAsync(ImportExportLog log);
    void Update(ImportExportLog log);
    void Delete(ImportExportLog log);

    // Search and Filter
    Task<IEnumerable<ImportExportLog>> SearchAsync(string searchTerm);
    Task<(IEnumerable<ImportExportLog> Items, int TotalCount)> GetFilteredAsync(
        string? operationType = null,
        string? correspondenceType = null,
        string? status = null,
        string? importType = null,
        string? exportFormat = null,
        DateTime? operationDateFrom = null,
        DateTime? operationDateTo = null,
        Guid? operatedByUserId = null);
    Task<(IEnumerable<ImportExportLog> Items, int TotalCount)> GetFilteredAsync(ImportExportLogFilterDto filter);

    // Operation-specific queries
    Task<IEnumerable<ImportExportLog>> GetImportsAsync();
    Task<IEnumerable<ImportExportLog>> GetExportsAsync();
    Task<IEnumerable<ImportExportLog>> GetIncomingImportsAsync();
    Task<IEnumerable<ImportExportLog>> GetOutgoingImportsAsync();
    Task<IEnumerable<ImportExportLog>> GetSuccessfulImportsAsync();
    Task<IEnumerable<ImportExportLog>> GetFailedImportsAsync();
    Task<IEnumerable<ImportExportLog>> GetRolledBackImportsAsync();

    // User-specific queries
    Task<IEnumerable<ImportExportLog>> GetByUserAsync(Guid userId);

    // Date range queries
    Task<IEnumerable<ImportExportLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);

    // Specific queries
    Task<bool> ExistsAsync(Guid id);
    Task<ImportExportLog?> GetLatestImportAsync(string correspondenceType);
    Task<ImportExportLog?> GetLatestExportAsync(string correspondenceType);

    // Statistics
    Task<int> GetTotalImportsAsync();
    Task<int> GetTotalExportsAsync();
    Task<int> GetSuccessfulImportsCountAsync();
    Task<int> GetFailedImportsCountAsync();
    Task<int> GetRolledBackImportsCountAsync();
    Task<double> GetAverageImportSuccessRateAsync();
    Task<Dictionary<string, int>> GetImportsByTypeAsync();
    Task<Dictionary<string, int>> GetImportsByStatusAsync();
    Task<Dictionary<string, int>> GetExportsByFormatAsync();

    // Bulk operations
    Task AddRangeAsync(IEnumerable<ImportExportLog> logs);
    void UpdateRange(IEnumerable<ImportExportLog> logs);
    void DeleteRange(IEnumerable<ImportExportLog> logs);

    // Include operations
    System.Linq.IQueryable<ImportExportLog> IncludeNavigationProperties();
}
