namespace IIROSA.Domain.Entities.Base;

/// <summary>
/// Base class for main entities with GUID primary key
/// Inherits from Framework.Core.Data.FullAuditedEntityBase<Guid>
/// </summary>
public abstract class FullAuditedEntity : global::Framework.Core.Data.FullAuditedEntityBase<Guid>
{
    protected FullAuditedEntity()
    {
        Id = Guid.NewGuid();
        CreatedOn = DateTime.UtcNow;
        UpdatedOn = DateTime.UtcNow;
    }
}

/// <summary>
/// Base class for lookup entities with default integer primary key
/// Inherits from Framework.Core.Data.LookupEntityBase (which has int Id)
/// Use this for standard lookup entities with int Id
/// </summary>
public abstract class LookupEntity : global::Framework.Core.Data.LookupEntityBase
{
    // Id: int (inherited from Framework.Core.Data.LookupEntityBase)
    // NameAr, NameEn, Name, IsActive (inherited)
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn (inherited)

    protected LookupEntity()
    {
        IsActive = true;
    }
}

/// <summary>
/// Base class for lookup entities with generic primary key type
/// Inherits from Framework.Core.Data.LookupEntityBase<TKey>
/// Use this when you need specific Id type: int, Guid, string, etc.
/// Example: public class MyLookup : LookupEntity<Guid>
/// </summary>
public abstract class LookupEntity<TKey> : global::Framework.Core.Data.LookupEntityBase<TKey> where TKey : struct
{
    // Id<TKey> (inherited from Framework.Core.Data.LookupEntityBase<TKey>)
    // NameAr, NameEn, Name, IsActive (inherited)
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn (inherited)

    protected LookupEntity()
    {
        IsActive = true;
    }
}
