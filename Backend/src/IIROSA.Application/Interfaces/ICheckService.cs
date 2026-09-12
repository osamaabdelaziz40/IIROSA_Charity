using IIROSA.Application.DTOs.CheckManagement;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// General cheques service (chapter 16, UC-CHQ-01..10).
/// Every read and write is scoped to the caller's charity from the JWT; a head-office
/// role may pass an explicit charity id on reads.
/// </summary>
public interface ICheckService
{
    /// <summary>UC-CHQ-01 — paged cheque register.</summary>
    Task<CheckPagedResult<CheckListDto>> GetChecksAsync(CheckFilterDto filter);

    /// <summary>
    /// Register statistics for the band above the cheque register (§16.S.1, UC-CHQ-01) —
    /// same caller scope as the register read: charity pin or head-office country pin.
    /// </summary>
    Task<CheckStatisticsDto> GetStatisticsAsync();

    /// <summary>The register (§16.S.1 grid columns) as an Excel workbook — every filtered row.</summary>
    Task<byte[]> ExportChecksToExcelAsync(CheckFilterDto filter);

    /// <summary>UC-CHQ-03 — single cheque for review or edit.</summary>
    Task<CheckDetailDto?> GetCheckByIdAsync(Guid id);

    /// <summary>UC-CHQ-02 — issue a cheque; stamps the owning charity and the Arabic words.</summary>
    Task<CheckDetailDto> CreateCheckAsync(CreateCheckDto dto);

    /// <summary>UC-CHQ-04 — update a cheque before clearing.</summary>
    Task<CheckDetailDto> UpdateCheckAsync(UpdateCheckDto dto);

    /// <summary>UC-CHQ-09 — filtered cheque statement بيان الشيكات.</summary>
    Task<CheckStatementDto> GetStatementAsync(CheckFilterDto filter);

    /// <summary>UC-CHQ-07 — convert an amount to its Arabic words تفقيط.</summary>
    Task<AmountInWordsDto> GetAmountInWordsAsync(decimal amount, string currency);
}
