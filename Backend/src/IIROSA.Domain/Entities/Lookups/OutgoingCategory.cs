using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

/// <summary>
/// Outgoing Letter Category Lookup
/// Used to classify outgoing letters (Official, Internal, External, etc.)
/// </summary>
public class OutgoingCategory : LookupEntity
{
    // Id: int (inherited from LookupEntity)
    // NameAr, NameEn, Name (inherited)
    // IsActive, CreatedBy, CreatedOn, UpdatedBy, UpdatedOn (inherited)

    public string? Description { get; set; }

    // Navigation
    public virtual ICollection<Outgoing> OutgoingLetters { get; set; } = new List<Outgoing>();
}
