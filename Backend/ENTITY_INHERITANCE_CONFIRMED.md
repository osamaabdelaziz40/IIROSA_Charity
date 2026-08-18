# ✅ Entity Inheritance Confirmed - Using Framework.Core.Data

## Base Classes from Framework.Core.Data

### Main Entities (FullAuditedEntityBase<Guid>)
All main business entities inherit from `FullAuditedEntityBase<Guid>` which provides:
- `Id` (Guid)
- `CreatedBy` (string)
- `CreatedOn` (DateTime)
- `UpdatedBy` (string)
- `UpdatedOn` (DateTime?)
- Inherits from `Framework.Core.Data.FullAuditedEntityBase<Guid>`

### Lookup Entities (LookupEntityBase<int>)
All lookup/reference entities inherit from `LookupEntityBase` which provides:
- `Id` (int) - already defined in Framework.Core.Data.LookupEntityBase
- `NameAr` (string)
- `NameEn` (string)
- `Name` (string - computed property, returns NameAr or NameEn based on culture)
- `IsActive` (bool)
- `CreatedBy`, `CreatedOn`, `UpdatedBy`, `UpdatedOn` (inherited from FullAuditedEntityBase)
- Inherits from `Framework.Core.Data.LookupEntityBase`

---

## Entity Inheritance Tree

```
Framework.Core.Data.FullAuditedEntityBase<Guid>
    │
    └── IIROSA.Domain.Entities.Base.FullAuditedEntity
            │
            ├── Charity (Id: Guid)
            ├── Employee (Id: Guid)
            ├── Orphan (Id: Guid)
            ├── Family (Id: Guid)
            ├── Sponsor (Id: Guid)
            └── PeriodicOrphanReport (Id: Guid)

Framework.Core.Data.LookupEntityBase
    │
    └── IIROSA.Domain.Entities.Base.LookupEntity
            │
            ├── Country (Id: int)
            ├── City (Id: int)
            └── Department (Id: int)
```

---

## Entity Examples

### Main Entity (Guid Id)
```csharp
using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities;

public class Charity : FullAuditedEntity  // Inherits FullAuditedEntityBase<Guid>
{
    // Id: Guid is inherited
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn are inherited

    public string Code { get; set; }
    public string Name { get; set; }
    // ... other properties
}
```

### Lookup Entity (int Id)
```csharp
using IIROSA.Domain.Entities.Base;

namespace IIROSA.Domain.Entities.Lookups;

public class Country : LookupEntity  // Inherits Framework.Core.Data.LookupEntityBase
{
    // Id: int is inherited from Framework.Core.Data.LookupEntityBase
    // NameAr, NameEn, Name, IsActive are inherited
    // CreatedBy, CreatedOn, UpdatedBy, UpdatedOn are inherited from FullAuditedEntityBase

    public string? IsoCode { get; set; }
    // ... other properties
}
```

---

## Summary

✅ All main entities inherit from `Framework.Core.Data.FullAuditedEntityBase<Guid>`
✅ All lookup entities inherit from `Framework.Core.Data.LookupEntityBase<int>` (via LookupEntity wrapper)
✅ Audit fields are automatically included (CreatedBy, CreatedOn, UpdatedBy, UpdatedOn)
✅ Lookup entities have bilingual support (NameAr, NameEn, Name)
✅ No need to manually add audit fields - they're inherited from Framework.Core.Data

---

## Files Updated

1. ✅ `Backend/src/Domain/Entities/Base/FullAuditedEntityBase.cs` - Base wrapper classes
2. ✅ `Backend/src/Domain/Entities/Charity.cs` - Inherits FullAuditedEntity
3. ✅ `Backend/src/Domain/Entities/Employee.cs` - Inherits FullAuditedEntity
4. ✅ `Backend/src/Domain/Entities/Orphan.cs` - Inherits FullAuditedEntity
5. ✅ `Backend/src/Domain/Entities/Family.cs` - Inherits FullAuditedEntity
6. ✅ `Backend/src/Domain/Entities/Sponsor.cs` - Inherits FullAuditedEntity
7. ✅ `Backend/src/Domain/Entities/PeriodicOrphanReport.cs` - Inherits FullAuditedEntity
8. ✅ `Backend/src/Domain/Entities/Lookups/Country.cs` - Inherits LookupEntity
9. ✅ `Backend/src/Domain/Entities/Lookups/City.cs` - Inherits LookupEntity
10. ✅ `Backend/src/Domain/Entities/Lookups/Department.cs` - Inherits LookupEntity

**All entities now properly inherit from Framework.Core.Data base classes!** ✨
