using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// Incoming Letter Repository Interface (epic 16, UC-COR-01…09)
/// </summary>
public interface IIncomingRepository : IRepository<Incoming>
{
    /// <summary>
    /// Paged register read with the §21.S.1 criteria applied server-side
    /// (scope pinning happens in the service, before this call).
    /// </summary>
    Task<(IEnumerable<Incoming> Items, int TotalCount)> GetPagedAsync(
        IncomingFilterCriteria criteria,
        int pageNumber,
        int pageSize);

    /// <summary>Detail read with the navigations the §21.S.2 view renders.</summary>
    Task<Incoming?> GetWithDetailsAsync(Guid id);

    /// <summary>Letter numbers are unique per charity + year (16-4).</summary>
    Task<bool> IsLetterNumberUniqueAsync(string letterNumber, Guid? charityId, int year, Guid? excludeId = null);

    /// <summary>Reply-count of an outgoing letter — the 16-7 delete guard.</summary>
    Task<int> CountOutgoingRepliesAsync(Guid incomingId);

    /// <summary>
    /// Next serial in the charity + year sequence (UC-COR-03). Advisory when shown in the
    /// form; CreateAsync re-derives it inside its transaction to close the Max+1 race.
    /// </summary>
    Task<int> GetNextSerialAsync(Guid? charityId, int year);
}

/// <summary>
/// The §21.S.1 search criteria, already scope-pinned by the service.
/// </summary>
public class IncomingFilterCriteria
{
    public string? SearchTerm { get; set; }
    public int? Serial { get; set; }
    public string? LetterNumber { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? CharityId { get; set; }

    /// <summary>Country pin (service-set from the caller's claims) — rides through the owning charity.</summary>
    public int? CountryId { get; set; }

    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
