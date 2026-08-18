namespace IIROSA.Application.DTOs.IncomingOutgoing;

/// <summary>
/// Base Outgoing Letter DTO
/// </summary>
public class OutgoingDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? OutGoingNumber { get; set; }
    public string OutGoingId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int? Year { get; set; }
    public int? Fk_DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? IncomingId { get; set; }
    public string? IncomingLetterSubject { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// Create Outgoing Letter DTO
/// </summary>
public class CreateOutgoingDto
{
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? OutGoingNumber { get; set; }
    public string OutGoingId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int? Year { get; set; }
    public int? Fk_DepartmentId { get; set; }
    public Guid? UploadedFileId { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public Guid? IncomingId { get; set; }
}

/// <summary>
/// Update Outgoing Letter DTO
/// </summary>
public class UpdateOutgoingDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? OutGoingNumber { get; set; }
    public string OutGoingId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public int? Year { get; set; }
    public int? Fk_DepartmentId { get; set; }
    public Guid? UploadedFileId { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public Guid? IncomingId { get; set; }
}

/// <summary>
/// Outgoing Letter List DTO
/// </summary>
public class OutgoingListDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string OutGoingId { get; set; } = string.Empty;
    public string? OutGoingNumber { get; set; }
    public string? DepartmentName { get; set; }
    public string? CategoryName { get; set; }
    public int? Year { get; set; }
    public bool HasReply { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Outgoing Letter Filter DTO
/// Implements filter requirements from UC-12.6
/// </summary>
public class OutgoingFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public int? Year { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public bool? HasReply { get; set; }
    public string? SortBy { get; set; } = "Date"; // Date, Serial, Subject, OutGoingId
    public string? SortOrder { get; set; } = "Descending"; // Ascending, Descending
}
