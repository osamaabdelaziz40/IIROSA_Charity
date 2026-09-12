using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Outgoing Letter Repository Interface (epic 16, UC-COR-10…19)
/// </summary>
public interface IOutgoingRepository : IRepository<Outgoing>
{
    /// <summary>
    /// Paged register read with the §21.S.4 criteria applied server-side
    /// (scope pinning happens in the service, before this call).
    /// </summary>
    Task<(IEnumerable<Outgoing> Items, int TotalCount)> GetPagedAsync(
        OutgoingFilterCriteria criteria,
        int pageNumber,
        int pageSize);

    /// <summary>
    /// Register statistics for the band above the §21.S.4 grid — one grouped round-trip
    /// over the same filtered query as <see cref="GetPagedAsync"/> (scope pinning happens
    /// in the service, before this call; ThisYear matches the register's Year column).
    /// </summary>
    Task<(int Total, int ThisYear, int AddedThisMonth)> GetRegisterStatisticsAsync(
        OutgoingFilterCriteria criteria);

    /// <summary>Detail read with the navigations the §21.S.5 view renders.</summary>
    Task<Outgoing?> GetWithDetailsAsync(Guid id);

    /// <summary>
    /// Next serial in the charity + year sequence (UC-COR-12). Advisory when shown in the
    /// form; CreateAsync re-derives it inside its transaction to close the Max+1 race.
    /// </summary>
    Task<int> GetNextSerialAsync(Guid? charityId, int year);
}

/// <summary>
/// The §21.S.4 search criteria, already scope-pinned by the service.
/// </summary>
public class OutgoingFilterCriteria
{
    public string? SearchTerm { get; set; }
    public int? Serial { get; set; }
    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public int? Year { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? HasReply { get; set; }
    public Guid? CharityId { get; set; }

    /// <summary>Country pin (service-set from the caller's claims) — rides through the owning charity.</summary>
    public int? CountryId { get; set; }

    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
