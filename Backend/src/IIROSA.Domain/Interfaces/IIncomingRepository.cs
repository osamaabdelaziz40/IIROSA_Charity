using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Incoming Letter Repository Interface
/// Implements data access for UC-12.1 through UC-12.5
/// </summary>
public interface IIncomingRepository : IRepository<Incoming>
{
    // Common CRUD wrapper with full filtering (UC-12.1)
    Task<(IEnumerable<Incoming> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? departmentId = null,
        string? status = null,
        int? year = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        string? sortBy = null,
        string? sortOrder = null);

    // Business Logic Queries
    Task<bool> IsIncomingIdUniqueAsync(string incomingId, Guid? excludeId = null);
    Task<bool> IsLetterNumberUniqueAsync(string letterNumber, int? departmentId, int? year, Guid? excludeId = null);
    Task<IEnumerable<Incoming>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Incoming>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Incoming>> GetByStatusAsync(string status);
    Task<IEnumerable<Incoming>> GetByYearAsync(int year);
    Task<IEnumerable<Incoming>> GetByUserAsync(Guid userId);

    // Import/Export Support
    Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null);
    Task<string> GenerateSerialTextAsync(int serial);
}
