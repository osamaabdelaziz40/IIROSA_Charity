using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Outgoing Letter Service Interface (epic 16, UC-COR-10…19)
/// </summary>
public interface IOutgoingService
{
    Task<OutgoingDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<OutgoingListDto> Items, int TotalCount, int Page)> GetPagedAsync(OutgoingFilterDto filter);

    /// <summary>Register statistics for the band above the §21.S.4 grid — the register read's caller scope.</summary>
    Task<OutgoingStatisticsDto> GetStatisticsAsync();

    Task<OutgoingDto> CreateAsync(CreateOutgoingDto dto);
    Task<OutgoingDto> UpdateAsync(Guid id, UpdateOutgoingDto dto);
    Task DeleteAsync(Guid id, Guid? deletedBy);

    // Catalogues + serials
    Task<IEnumerable<OutgoingCategoryOptionDto>> GetAvailableCategoriesAsync();
    Task<NextSerialDto> GetNextSerialAsync(int? year, Guid? charityId);

    // Orphan report attachment (UC-COR-18)
    Task<OutgoingOrphansDto> GetOrphansAsync(Guid outgoingId);
    Task AttachOrphanAsync(Guid outgoingId, Guid orphanId);
    Task DetachOrphanAsync(Guid outgoingId, Guid orphanId);

    // Orphans-by-outgoing-letter report (UC-COR-19)
    Task<(IEnumerable<OutgoingOrphanReportRowDto> Items, int TotalCount, int Page)> GetOrphanReportAsync(OutgoingOrphanReportFilterDto filter);
}
