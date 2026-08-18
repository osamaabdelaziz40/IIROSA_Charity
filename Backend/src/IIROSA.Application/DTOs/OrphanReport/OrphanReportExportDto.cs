namespace IIROSA.Application.DTOs.OrphanReport;

/// <summary>
/// Export options for Orphan Report - UC-6.7
/// </summary>
public class OrphanReportExportDto
{
    /// <summary>
    /// Export format - UC-6.7
    /// Values: Excel, PDF
    /// </summary>
    public string ExportFormat { get; set; } = "Excel";

    /// <summary>
    /// File name (optional, auto-generated if not provided)
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Include all data fields
    /// </summary>
    public bool IncludeAllFields { get; set; } = false;

    /// <summary>
    /// Include summary only
    /// </summary>
    public bool IncludeSummaryOnly { get; set; } = false;
}
