using IIROSA.Domain.Contracts.Persistence;
using IIROSA.Domain.Entities;

namespace IIROSA.Domain.Interfaces;

/// <summary>
/// A flat orphan row for the §21.S.6 grids (UC-COR-18) — code, name, guarantor and
/// kinship served together, no client-side joins. Guarantor/kinship ride with the
/// family register (HeadOfFamily / ProviderType).
/// </summary>
public class OrphanCandidateRow
{
    public Guid OrphanId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? GuarantorName { get; set; }
    public string? Kinship { get; set; }
}

/// <summary>
/// One row of the §21.S.7 report (UC-COR-19).
/// </summary>
public class OutgoingOrphanReportRow
{
    public Guid OutgoingId { get; set; }
    public int? Serial { get; set; }
    public int? Year { get; set; }
    public DateTime? LetterDate { get; set; }
    public string? CharityName { get; set; }
    public int OrphanCount { get; set; }
    public bool OrphanAttached { get; set; }
}

/// <summary>
/// Outgoing ↔ Orphan report link repository (epic 16, UC-COR-18 / UC-COR-19)
/// </summary>
public interface IOutgoingOrphanReportRepository : IRepository<OutgoingOrphanReport>
{
    /// <summary>Live links of a letter, with the orphan navigation loaded.</summary>
    Task<IEnumerable<OutgoingOrphanReport>> GetByOutgoingAsync(Guid outgoingId);

    /// <summary>Whether the orphan is already attached to ANY live letter (BR-26).</summary>
    Task<bool> IsOrphanAttachedAsync(Guid orphanId);

    /// <summary>The live link row for (letter, orphan), or null.</summary>
    Task<OutgoingOrphanReport?> FindByLetterAndOrphanAsync(Guid outgoingId, Guid orphanId);

    /// <summary>
    /// The charity's orphans not attached to any live letter (BR-26 + BR-27 — the §21.S.6
    /// unattached grid). Null charityId means HQ without a letter scope: all unattached.
    /// </summary>
    Task<IEnumerable<OrphanCandidateRow>> GetUnattachedOrphansAsync(Guid? charityId);

    /// <summary>
    /// The §21.S.7 report projection: outgoing letters matching the criteria with the
    /// attached flag for the given child code's orphan and per-letter orphan counts.
    /// ChildCode is mandatory (service enforces).
    /// </summary>
    Task<(IEnumerable<OutgoingOrphanReportRow> Rows, int TotalCount)> GetOrphanReportAsync(
        int? serial,
        int? year,
        Guid? charityId,
        int? countryId,
        DateTime? dateFrom,
        DateTime? dateTo,
        string childCode,
        int pageNumber,
        int pageSize);
}
