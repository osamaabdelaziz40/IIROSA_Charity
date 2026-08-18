using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Outgoing Letter Repository Interface
/// Implements data access for UC-12.6 through UC-12.10
/// </summary>
public interface IOutgoingRepository : IRepository<Outgoing>
{
    // Common CRUD wrapper with full filtering (UC-12.6)
    Task<(IEnumerable<Outgoing> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        int? departmentId = null,
        int? categoryId = null,
        int? year = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        bool? hasReply = null,
        string? sortBy = null,
        string? sortOrder = null);

    // Business Logic Queries
    Task<bool> IsOutgoingIdUniqueAsync(string outgoingId, Guid? excludeId = null);
    Task<IEnumerable<Outgoing>> GetByDepartmentAsync(int departmentId);
    Task<IEnumerable<Outgoing>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<Outgoing>> GetByYearAsync(int year);
    Task<IEnumerable<Outgoing>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Outgoing>> GetRepliesToIncomingAsync(Guid incomingId);
    Task<IEnumerable<Outgoing>> GetByUserAsync(Guid userId);

    // Import/Export Support
    Task<int> GetNextSerialNumberAsync(int? departmentId = null, int? year = null);
}
