using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using System.Linq.Expressions;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Mission Repository Interface
/// Defines data access operations for Mission entity
/// Implements use cases UC-8.1 through UC-8.13
/// </summary>
public interface IMissionRepository : IRepository<Mission>
{
    // ========== Mission-Specific Queries ==========

    /// <summary>
    /// Get missions assigned to a specific user (UC-8.12: My Missions)
    /// </summary>
    Task<IEnumerable<Mission>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Get missions by completion status
    /// </summary>
    Task<IEnumerable<Mission>> GetByStatusAsync(bool isCompleted);

    /// <summary>
    /// Get missions by mission type (UC-8.3)
    /// </summary>
    Task<IEnumerable<Mission>> GetByMissionTypeAsync(int missionTypeId);

    /// <summary>
    /// Get missions by mission time type (UC-8.4)
    /// </summary>
    Task<IEnumerable<Mission>> GetByMissionTimeTypeAsync(int missionTimeTypeId);

    /// <summary>
    /// Get missions within a date range
    /// </summary>
    Task<IEnumerable<Mission>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get overdue missions (past due date and not completed)
    /// </summary>
    Task<IEnumerable<Mission>> GetOverdueMissionsAsync();

    /// <summary>
    /// Get missions by location filters
    /// </summary>
    Task<IEnumerable<Mission>> GetByLocationAsync(int? countryId, int? regionId, int? centerId);

    // ========== Filtering and Pagination ==========

    /// <summary>
    /// Get paginated missions with optional filtering and sorting (UC-8.10)
    /// </summary>
    Task<(IEnumerable<Mission> Items, int TotalCount)> GetMissionsPagedAsync(
        Expression<Func<Mission, bool>>? filter = null,
        Func<IQueryable<Mission>, IOrderedQueryable<Mission>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    // ========== Navigation Properties ==========

    /// <summary>
    /// Include all navigation properties for eager loading
    /// </summary>
    IQueryable<Mission> IncludeNavigationProperties();

    /// <summary>
    /// Include specific navigation properties
    /// </summary>
    IQueryable<Mission> IncludeSpecificNavigationProperties(params Expression<Func<Mission, object>>[] includes);
}
