using IIROSA.Application.DTOs.ImportExport;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// Import/Export Repository Implementation
/// Provides data access operations for ImportExportLog entity
/// </summary>
public class ImportExportRepository : IImportExportRepository
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<ImportExportLog> _dbSet;

    public ImportExportRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<ImportExportLog>();
    }

    #region Basic CRUD

    public async Task<ImportExportLog?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<ImportExportLog>> GetAllAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ImportExportLog> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var query = _dbSet.Where(l => !l.IsDeleted);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.OperationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(ImportExportLog log)
    {
        await _dbSet.AddAsync(log);
    }

    public void Update(ImportExportLog log)
    {
        _context.Entry(log).State = EntityState.Modified;
    }

    public void Delete(ImportExportLog log)
    {
        log.IsDeleted = true;
        log.DeletedOn = DateTime.UtcNow;
        _dbSet.Update(log);
    }

    #endregion

    #region Search and Filter

    public async Task<IEnumerable<ImportExportLog>> SearchAsync(string searchTerm)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        (l.FileName.Contains(searchTerm) ||
                         (l.Notes != null && l.Notes.Contains(searchTerm))))
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<(IEnumerable<ImportExportLog> Items, int TotalCount)> GetFilteredAsync(
        string? operationType = null,
        string? correspondenceType = null,
        string? status = null,
        string? importType = null,
        string? exportFormat = null,
        DateTime? operationDateFrom = null,
        DateTime? operationDateTo = null,
        Guid? operatedByUserId = null)
    {
        var query = _dbSet.Where(l => !l.IsDeleted);

        if (!string.IsNullOrWhiteSpace(operationType))
            query = query.Where(l => l.OperationType == operationType);

        if (!string.IsNullOrWhiteSpace(correspondenceType))
            query = query.Where(l => l.CorrespondenceType == correspondenceType);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);

        if (!string.IsNullOrWhiteSpace(importType))
            query = query.Where(l => l.ImportType == importType);

        if (!string.IsNullOrWhiteSpace(exportFormat))
            query = query.Where(l => l.ExportFormat == exportFormat);

        if (operationDateFrom.HasValue)
            query = query.Where(l => l.OperationDate >= operationDateFrom.Value);

        if (operationDateTo.HasValue)
            query = query.Where(l => l.OperationDate <= operationDateTo.Value);

        if (operatedByUserId.HasValue)
            query = query.Where(l => l.OperatedByUserId == operatedByUserId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<(IEnumerable<ImportExportLog> Items, int TotalCount)> GetFilteredAsync(ImportExportLogFilterDto filter)
    {
        var query = _dbSet.Where(l => !l.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(l =>
                l.FileName.Contains(filter.SearchTerm) ||
                (l.Notes != null && l.Notes.Contains(filter.SearchTerm)));
        }

        // Apply filters
        if (!string.IsNullOrWhiteSpace(filter.OperationType))
            query = query.Where(l => l.OperationType == filter.OperationType);

        if (!string.IsNullOrWhiteSpace(filter.CorrespondenceType))
            query = query.Where(l => l.CorrespondenceType == filter.CorrespondenceType);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(l => l.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.ImportType))
            query = query.Where(l => l.ImportType == filter.ImportType);

        if (!string.IsNullOrWhiteSpace(filter.ExportFormat))
            query = query.Where(l => l.ExportFormat == filter.ExportFormat);

        // Apply date range filters
        if (filter.OperationDateFrom.HasValue)
            query = query.Where(l => l.OperationDate >= filter.OperationDateFrom.Value);

        if (filter.OperationDateTo.HasValue)
            query = query.Where(l => l.OperationDate <= filter.OperationDateTo.Value);

        // Apply user filter
        if (filter.OperatedByUserId.HasValue)
            query = query.Where(l => l.OperatedByUserId == filter.OperatedByUserId.Value);

        // Get total count
        var totalCount = await query.CountAsync();

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(filter.SortBy))
        {
            var sortDirection = filter.SortDescending ? "descending" : "ascending";
            try
            {
                query = query.OrderBy($"{filter.SortBy} {sortDirection}");
            }
            catch
            {
                query = query.OrderBy(l => l.OperationDate);
            }
        }
        else
        {
            query = query.OrderByDescending(l => l.OperationDate);
        }

        // Apply pagination
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    #endregion

    #region Operation-specific queries

    public async Task<IEnumerable<ImportExportLog>> GetImportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.OperationType == "Import")
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportExportLog>> GetExportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.OperationType == "Export")
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportExportLog>> GetIncomingImportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationType == "Import" &&
                        l.CorrespondenceType == "Incoming")
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportExportLog>> GetOutgoingImportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationType == "Import" &&
                        l.CorrespondenceType == "Outgoing")
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportExportLog>> GetSuccessfulImportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationType == "Import" &&
                        (l.Status == "Success" || l.Status == "PartialSuccess"))
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportExportLog>> GetFailedImportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationType == "Import" &&
                        l.Status == "Failed")
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ImportExportLog>> GetRolledBackImportsAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.Status == "RolledBack")
            .OrderByDescending(l => l.RollbackDate)
            .ToListAsync();
    }

    #endregion

    #region User-specific queries

    public async Task<IEnumerable<ImportExportLog>> GetByUserAsync(Guid userId)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.OperatedByUserId == userId)
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    #endregion

    #region Date range queries

    public async Task<IEnumerable<ImportExportLog>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationDate >= fromDate &&
                        l.OperationDate <= toDate)
            .OrderByDescending(l => l.OperationDate)
            .ToListAsync();
    }

    #endregion

    #region Specific queries

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _dbSet.AnyAsync(l => l.Id == id && !l.IsDeleted);
    }

    public async Task<ImportExportLog?> GetLatestImportAsync(string correspondenceType)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationType == "Import" &&
                        l.CorrespondenceType == correspondenceType)
            .OrderByDescending(l => l.OperationDate)
            .FirstOrDefaultAsync();
    }

    public async Task<ImportExportLog?> GetLatestExportAsync(string correspondenceType)
    {
        return await _dbSet
            .Where(l => !l.IsDeleted &&
                        l.OperationType == "Export" &&
                        l.CorrespondenceType == correspondenceType)
            .OrderByDescending(l => l.OperationDate)
            .FirstOrDefaultAsync();
    }

    #endregion

    #region Statistics

    public async Task<int> GetTotalImportsAsync()
    {
        return await _dbSet.CountAsync(l => !l.IsDeleted && l.OperationType == "Import");
    }

    public async Task<int> GetTotalExportsAsync()
    {
        return await _dbSet.CountAsync(l => !l.IsDeleted && l.OperationType == "Export");
    }

    public async Task<int> GetSuccessfulImportsCountAsync()
    {
        return await _dbSet.CountAsync(l => !l.IsDeleted &&
                                         l.OperationType == "Import" &&
                                         (l.Status == "Success" || l.Status == "PartialSuccess"));
    }

    public async Task<int> GetFailedImportsCountAsync()
    {
        return await _dbSet.CountAsync(l => !l.IsDeleted &&
                                         l.OperationType == "Import" &&
                                         l.Status == "Failed");
    }

    public async Task<int> GetRolledBackImportsCountAsync()
    {
        return await _dbSet.CountAsync(l => !l.IsDeleted && l.Status == "RolledBack");
    }

    public async Task<double> GetAverageImportSuccessRateAsync()
    {
        var imports = await _dbSet
            .Where(l => !l.IsDeleted && l.OperationType == "Import" && l.TotalRows > 0)
            .ToListAsync();

        if (!imports.Any())
            return 0;

        return imports.Average(l => l.SuccessRate);
    }

    public async Task<Dictionary<string, int>> GetImportsByTypeAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.OperationType == "Import")
            .GroupBy(l => l.ImportType ?? "Unknown")
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetImportsByStatusAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.OperationType == "Import")
            .GroupBy(l => l.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetExportsByFormatAsync()
    {
        return await _dbSet
            .Where(l => !l.IsDeleted && l.OperationType == "Export")
            .GroupBy(l => l.ExportFormat ?? "Unknown")
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }

    #endregion

    #region Bulk Operations

    public async Task AddRangeAsync(IEnumerable<ImportExportLog> logs)
    {
        await _dbSet.AddRangeAsync(logs);
    }

    public void UpdateRange(IEnumerable<ImportExportLog> logs)
    {
        _dbSet.UpdateRange(logs);
    }

    public void DeleteRange(IEnumerable<ImportExportLog> logs)
    {
        foreach (var log in logs)
        {
            log.IsDeleted = true;
            log.DeletedOn = DateTime.UtcNow;
        }
        _dbSet.UpdateRange(logs);
    }

    #endregion

    #region Include Operations

    public System.Linq.IQueryable<ImportExportLog> IncludeNavigationProperties()
    {
        return _dbSet;
    }

    #endregion
}
