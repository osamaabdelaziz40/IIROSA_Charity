namespace IIROSA.Application.DTOs.IncomingOutgoing;

/// <summary>
/// Incoming letter detail DTO (epic 16, UC-COR-05) — clean camelCase wire: the FK_* entity
/// names surface as departmentId / assignedUserId / serialTxt, never fk_DepartmentId.
/// </summary>
public class IncomingDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string? SerialTxt { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? LetterDescription { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public Guid? CharityId { get; set; }
    public string? CharityName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// Register an incoming letter (epic 16, UC-COR-04 / §21.S.2). Serial is allocated
/// server-side per charity + year — it is never accepted from the payload.
/// </summary>
public class CreateIncomingDto
{
    public DateTime? Date { get; set; }
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? DepartmentId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Status { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? LetterDescription { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Update an incoming letter (epic 16, UC-COR-06). Serial / charity ownership are immutable.
/// </summary>
public class UpdateIncomingDto
{
    public Guid Id { get; set; }
    public DateTime? Date { get; set; }
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public int? DepartmentId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Status { get; set; }
    public Guid? AssignedUserId { get; set; }
    public string? LetterDescription { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Incoming letter list row (epic 16, UC-COR-01 / §21.S.1 grid)
/// </summary>
public class IncomingListDto
{
    public Guid Id { get; set; }
    public int? Serial { get; set; }
    public string? SerialTxt { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? LetterNumber { get; set; }
    public DateTime? LetterDate { get; set; }
    public DateTime? Date { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? AssignedUserName { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public DateTime CreatedOn { get; set; }
}

/// <summary>
/// Incoming letter filter (epic 16, UC-COR-01 / UC-COR-02 — §21.S.1 search criteria).
/// CharityId is the HQ caller's explicit narrow; a charity caller is pinned server-side.
/// </summary>
public class IncomingFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public int? Serial { get; set; }
    public string? LetterNumber { get; set; }
    public int? DepartmentId { get; set; }
    public string? Status { get; set; }
    public int? Year { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? AssignedUserId { get; set; }
    public Guid? CharityId { get; set; }
    public string? SortBy { get; set; } = "Date";
    public string? SortOrder { get; set; } = "desc";
}

/// <summary>
/// Register statistics band shown above the §21.S.1 incoming grid (UC-COR-01).
/// Counts follow the caller's scope — the same ladder as the register read — so the
/// band and the grid beneath it can never disagree about what is counted.
/// </summary>
public class IncomingStatisticsDto
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
