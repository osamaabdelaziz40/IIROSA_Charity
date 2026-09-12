using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Incoming Letter Service Interface (epic 16, UC-COR-01…09)
/// </summary>
public interface IIncomingService
{
    Task<IncomingDto?> GetByIdAsync(Guid id);
    Task<(IEnumerable<IncomingListDto> Items, int TotalCount, int Page)> GetPagedAsync(IncomingFilterDto filter);

    /// <summary>Register statistics for the band above the §21.S.1 grid — the register read's caller scope.</summary>
    Task<IncomingStatisticsDto> GetStatisticsAsync();

    Task<IncomingDto> CreateAsync(CreateIncomingDto dto);
    Task<IncomingDto> UpdateAsync(Guid id, UpdateIncomingDto dto);
    Task DeleteAsync(Guid id, Guid? deletedBy);

    // Catalogues + serials
    Task<IEnumerable<CorrespondenceStatusDto>> GetAvailableStatusesAsync();
    Task<NextSerialDto> GetNextSerialAsync(int? year, Guid? charityId);

    // Employee attachment (UC-COR-09)
    Task<IncomingEmployeesDto> GetEmployeesAsync(Guid incomingId);
    Task AttachEmployeeAsync(Guid incomingId, Guid userId);
    Task DetachEmployeeAsync(Guid incomingId, Guid userId);
}
