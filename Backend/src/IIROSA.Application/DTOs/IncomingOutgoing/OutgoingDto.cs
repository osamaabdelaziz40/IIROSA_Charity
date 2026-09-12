namespace IIROSA.Application.DTOs.IncomingOutgoing;

/// <summary>
/// Outgoing letter detail DTO (epic 16, UC-COR-14)
/// </summary>
public class OutgoingDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? Year { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? IncomingId { get; set; }
    public string? IncomingLetterNumber { get; set; }
    public string? IncomingLetterSubject { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public List<OutgoingOrphanDto> Orphans { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// An orphan report attached to an outgoing letter (§21.S.6 — rendered on the
/// UC-COR-14 detail screen: أسم اليتيم · كود اليتيم).
/// </summary>
public class OutgoingOrphanDto
{
    public Guid OrphanId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// Register an outgoing letter (epic 16, UC-COR-13 / §21.S.5). Serial is allocated
/// server-side per charity + year — it is never accepted from the payload.
/// </summary>
public class CreateOutgoingDto
{
    public int? DepartmentId { get; set; }
    public DateTime? Date { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int? OutgoingCategoryId { get; set; }
    public Guid? IncomingId { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Update an outgoing letter (epic 16, UC-COR-15). Serial / charity ownership are immutable.
/// </summary>
public class UpdateOutgoingDto
{
    public Guid Id { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? Date { get; set; }
    public string Subject { get; set; } = string.Empty;
    public int? OutgoingCategoryId { get; set; }
    public Guid? IncomingId { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Outgoing letter list row (epic 16, UC-COR-10 / §21.S.4 grid)
/// </summary>
public class OutgoingListDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public int? Year { get; set; }
    public string? DepartmentName { get; set; }
    public int? OutgoingCategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool HasReply { get; set; }
    public string? IncomingLetterNumber { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Outgoing letter filter (epic 16, UC-COR-10 / UC-COR-11 — §21.S.4 search criteria)
/// </summary>
public class OutgoingFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public int? Serial { get; set; }
    public int? DepartmentId { get; set; }
    public int? CategoryId { get; set; }
    public int? Year { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool? HasReply { get; set; }
    public Guid? CharityId { get; set; }
    public string? SortBy { get; set; } = "Date";
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// Register statistics band shown above the §21.S.4 outgoing grid (UC-COR-10).
/// Counts follow the caller's scope — the same ladder as the register read — so the
/// band and the grid beneath it can never disagree about what is counted.
/// </summary>
public class OutgoingStatisticsDto
{
    public int Total { get; set; }

    /// <summary>
    /// Letters of the current year, matched on the register's <c>Year</c> column (stamped
    /// from the letter date at create) — the register's own year notion, not CreatedOn.
    /// Legacy rows carrying a NULL year fall out of this one card only.
    /// </summary>
    public int ThisYear { get; set; }

    /// <summary>Letters registered since the first day of the current (UTC) month.</summary>
    public int AddedThisMonth { get; set; }
}
