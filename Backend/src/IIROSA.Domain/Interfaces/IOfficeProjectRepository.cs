using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using System.Linq.Expressions;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// OfficeProject Repository Interface
/// Defines data access operations for OfficeProject entity
/// Implements use cases UC-7.1 through UC-7.14
/// </summary>
public interface IOfficeProjectRepository : IRepository<OfficeProject>
{
    // ========== OfficeProject-Specific Queries ==========

    /// <summary>
    /// Get projects assigned to a specific charity (UC-7.14)
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetByCharityIdAsync(Guid charityId);

    /// <summary>
    /// Get projects by completion status (UC-7.9, UC-7.10)
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetByStatusAsync(bool isFinished);

    /// <summary>
    /// Get projects by project type (UC-7.1)
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetByProjectTypeAsync(int projectTypeId);

    /// <summary>
    /// Get projects within a date range
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get ongoing projects (not finished)
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetOngoingProjectsAsync();

    /// <summary>
    /// Get completed projects
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetCompletedProjectsAsync();

    /// <summary>
    /// Get projects by location filters (UC-7.5, UC-7.10)
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetByLocationAsync(int? countryId, int? regionId, int? centerId);

    /// <summary>
    /// Get projects by donor name (UC-7.3)
    /// </summary>
    Task<IEnumerable<OfficeProject>> GetByDonorAsync(string donorName);

    // ========== Filtering and Pagination ==========

    /// <summary>
    /// Get paginated projects with optional filtering and sorting (UC-7.10)
    /// </summary>
    Task<(IEnumerable<OfficeProject> Items, int TotalCount)> GetProjectsPagedAsync(
        Expression<Func<OfficeProject, bool>>? filter = null,
        Func<IQueryable<OfficeProject>, IOrderedQueryable<OfficeProject>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    // ========== Navigation Properties ==========

    /// <summary>
    /// Include all navigation properties for eager loading
    /// </summary>
    IQueryable<OfficeProject> IncludeNavigationProperties();

    /// <summary>
    /// Include specific navigation properties
    /// </summary>
    IQueryable<OfficeProject> IncludeSpecificNavigationProperties(params Expression<Func<OfficeProject, object>>[] includes);
}
