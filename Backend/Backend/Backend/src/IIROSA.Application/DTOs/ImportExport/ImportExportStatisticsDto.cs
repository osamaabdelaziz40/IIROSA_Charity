namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Import/Export Statistics DTO - Summary statistics
/// </summary>
public class ImportExportStatisticsDto
{
    public int TotalImports { get; set; }
    public int TotalExports { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public int RolledBackImports { get; set; }
    public double AverageImportSuccessRate { get; set; }
    public Dictionary<string, int> ImportsByType { get; set; } = new();
    public Dictionary<string, int> ImportsByStatus { get; set; } = new();
    public Dictionary<string, int> ExportsByFormat { get; set; } = new();
}
