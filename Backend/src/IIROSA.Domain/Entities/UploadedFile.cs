using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

/// <summary>
/// Uploaded File Entity
/// Stores file metadata and links to letters
/// </summary>
public class UploadedFile : FullAuditedEntity
{
    public string FileName { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSize { get; set; }
    public string? FilePath { get; set; }
    public string? FileExtension { get; set; }

    // Navigation Collections
    public virtual ICollection<Incoming> IncomingLetters { get; set; } = new List<Incoming>();
    public virtual ICollection<Outgoing> OutgoingLetters { get; set; } = new List<Outgoing>();
    public virtual ICollection<ChildOutGoing> ChildOutgoingLetters { get; set; } = new List<ChildOutGoing>();
}
