using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Framework;

/// <summary>
/// Attachment Type lookup entity
/// Inherits from LookupEntityBase<int>
/// </summary>
public class AttachmentType : LookupEntity
{
    public string? DescriptionAr { get; set; }
    public string? DescriptionEn { get; set; }
    public string? Extensions { get; set; }
}
