namespace IIROSA.Application.DTOs.ImportExport;

/// <summary>
/// Import/Export Log Filter DTO - Used for filtering logs
/// </summary>
public class ImportExportLogFilterDto
{
    // Pagination
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Filters
    public string? OperationType { get; set; } // Import, Export
    public string? CorrespondenceType { get; set; } // Incoming, Outgoing
    public string? Status { get; set; }
    public string? ImportType { get; set; }
    public string? ExportFormat { get; set; }

    // Date Range Filters
    public DateTime? OperationDateFrom { get; set; }
    public DateTime? OperationDateTo { get; set; }

    // User Filter
    public Guid? OperatedByUserId { get; set; }

    // Search
    public string? SearchTerm { get; set; }

    // Sorting
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}
