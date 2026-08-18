namespace IIROSA.Application.DTOs.OrphanReport;

/// <summary>
/// Orphan Report History Item - UC-6.9
/// </summary>
public class OrphanReportHistoryDto
{
    public Guid ReportId { get; set; }
    public string? ReportName { get; set; }
    public DateTime ReportFromDate { get; set; }
    public DateTime ReportToDate { get; set; }
    public DateTime GeneratedDate { get; set; }
    public string? GeneratedBy { get; set; }
    public int OrphanCount { get; set; }
    public string? FiltersApplied { get; set; }

    // Actions
    public string ExportFormat { get; set; } = string.Empty; // Excel, PDF
}
