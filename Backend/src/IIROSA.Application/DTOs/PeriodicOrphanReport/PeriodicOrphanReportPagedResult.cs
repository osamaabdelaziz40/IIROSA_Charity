namespace IIROSA.Application.DTOs.PeriodicOrphanReport;

/// <summary>
/// Paged result wrapper for periodic orphan report reads (epic 9).
/// Wire shape: items / totalCount / page / pageSize / totalPages — the platform
/// envelope used by missions and every shipped list screen.
/// </summary>
public class PeriodicOrphanReportPagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((decimal)TotalCount / PageSize);
}
