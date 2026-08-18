namespace IIROSA.Application.DTOs.IncomingOutgoing;

/// <summary>
/// Child Outgoing Letter DTO
/// Represents follow-up letters linked to a parent Outgoing letter
/// </summary>
public class ChildOutGoingDto
{
    public Guid Id { get; set; }
    public Guid OutgoingId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Body { get; set; }
    public int? Year { get; set; }
    public int? Fk_DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? UploadedFileId { get; set; }
    public string? UploadedFileName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

/// <summary>
/// Create Child Outgoing Letter DTO
/// </summary>
public class CreateChildOutgoingDto
{
    public Guid OutgoingId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Body { get; set; }
    public int? Year { get; set; }
    public int? Fk_DepartmentId { get; set; }
    public Guid? UploadedFileId { get; set; }
}

/// <summary>
/// Update Child Outgoing Letter DTO
/// </summary>
public class UpdateChildOutgoingDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public DateTime? Date { get; set; }
    public string? Body { get; set; }
    public int? Year { get; set; }
    public int? Fk_DepartmentId { get; set; }
    public Guid? UploadedFileId { get; set; }
}
