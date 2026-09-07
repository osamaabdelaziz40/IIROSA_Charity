using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;
using System.Linq.Expressions;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// OfficeProject Repository Interface (UC-OFP-01…06)
/// </summary>
public interface IOfficeProjectRepository : IRepository<OfficeProject>
{
    /// <summary>
    /// Get a single project with all navigation properties loaded, so the detail DTO carries the
    /// lookup names and FK ids the edit form patches from (UC-OFP-04)
    /// </summary>
    Task<OfficeProject?> GetByIdWithDetailsAsync(Guid id);

    /// <summary>
    /// Get paginated projects with optional filtering and sorting; navigations included so list
    /// rows and the report carry their names (UC-OFP-01, UC-OFP-06)
    /// </summary>
    Task<(IEnumerable<OfficeProject> Items, int TotalCount)> GetProjectsPagedAsync(
        Expression<Func<OfficeProject, bool>>? filter = null,
        Func<IQueryable<OfficeProject>, IOrderedQueryable<OfficeProject>>? orderBy = null,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Eager-load the lookup/charity navigations shared by every read path
    /// </summary>
    IQueryable<OfficeProject> IncludeNavigationProperties();
}
