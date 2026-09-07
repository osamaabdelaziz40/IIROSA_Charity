using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using IIROSA.Infrastructure.Data;

namespace IIROSA.Infrastructure.Data.Repository;

/// <summary>
/// HqTransferDetail Repository Implementation (UC-TRF-08 — تفاصيل الحوالة).
/// The parent repository owns the parent-with-lines read (GetByIdWithLinesAsync); this
/// repository exists for the child add on the save path (UoW-only save).
/// </summary>
public class HqTransferDetailRepository : Repository<HqTransferDetail>, IHqTransferDetailRepository
{
    public HqTransferDetailRepository(ApplicationDbContext context) : base(context)
    {
    }
}
