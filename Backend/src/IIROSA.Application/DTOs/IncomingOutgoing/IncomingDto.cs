namespace IIROSA.Application.DTOs.IncomingOutgoing;

/// <summary>
/// Base Incoming Letter DTO
/// </summary>
public class IncomingDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string? Serial_Txt { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? IncomingNumber { get; set; }
    public string IncomingId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? LetterDescription { get; set; }
    public int? FK_DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? FK_UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? OutgoingId { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// Create Incoming Letter DTO
/// </summary>
public class CreateIncomingDto
{
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? IncomingNumber { get; set; }
    public string IncomingId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? LetterDescription { get; set; }
    public int? FK_DepartmentId { get; set; }
    public Guid? OutgoingId { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Update Incoming Letter DTO
/// </summary>
public class UpdateIncomingDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? IncomingNumber { get; set; }
    public string IncomingId { get; set; } = string.Empty;
    public string? Body { get; set; }
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? LetterDescription { get; set; }
    public int? FK_DepartmentId { get; set; }
    public Guid? OutgoingId { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Incoming Letter List DTO
/// </summary>
public class IncomingListDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string? Serial_Txt { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string IncomingId { get; set; } = string.Empty;
    public string? LetterNumber { get; set; }
    public string? Status { get; set; }
    public string? DepartmentName { get; set; }
    public int? Year { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Incoming Letter Filter DTO
/// Implements filter requirements from UC-12.1
/// </summary>
public class IncomingFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public string? SortBy { get; set; } = "Date"; // Date, Serial, Subject, LetterDate
    public string? SortOrder { get; set; } = "Descending"; // Ascending, Descending
}
