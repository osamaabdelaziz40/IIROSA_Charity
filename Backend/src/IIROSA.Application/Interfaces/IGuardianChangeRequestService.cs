using IIROSA.Application.DTOs;
using IIROSA.Application.DTOs.Family;

namespace IIROSA.Application.Interfaces;

/// <summary>
/// Guardian-change request aggregate service (UC-FAM-09 review queue / raise producer;
/// UC-FAM-10 approval extends the same aggregate in its own story).
/// Own aggregate, own service — architecture.md §5 one-service-per-aggregate.
/// </summary>
public interface IGuardianChangeRequestService
{
    /// <summary>
    /// The review queue: pending by default, newest first, paged. A <c>Charity</c>-role caller is
    /// scoped server-side to its own charity's requests; HQ roles see all and may filter by
    /// charity through <paramref name="filter"/>.
    /// </summary>
    Task<PagedResult<GuardianChangeRequestListDto>> GetRequestsAsync(
        GuardianChangeRequestFilterDto filter,
        Guid? userCharityId,
        string? userRole);

    /// <summary>
    /// Raise a guardian-change request for a family. Snapshots the current guardian (old) and the
    /// proposal (new) at raise time; stamps the raising charity from the family file — never the
    /// payload. Refuses a second pending request on the same family with the literal legacy
    /// message («تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب»).
    /// </summary>
    /// <exception cref="IIROSA.Application.Exceptions.NotFoundException">family does not exist</exception>
    /// <exception cref="IIROSA.Application.Exceptions.BusinessException">
    /// family belongs to another charity (Charity role), family has no charity, or a pending
    /// request already exists for the family
    /// </exception>
    Task<GuardianChangeRequestListDto> CreateRequestAsync(
        Guid familyId,
        CreateGuardianChangeRequestDto dto,
        string requestedByName,
        Guid? userCharityId,
        string? userRole);

    /// <summary>
    /// Record the head-office decision (UC-FAM-10 اعتماد تعديل العائل). Approving applies the
    /// proposed guardian to the family's provider row and stamps the request Approved; refusing
    /// stores the reason. Guardian application and the state transition commit in ONE transaction —
    /// an Approved request always means an applied family file, and vice versa.
    /// </summary>
    /// <exception cref="IIROSA.Application.Exceptions.NotFoundException">request does not exist</exception>
    /// <exception cref="IIROSA.Application.Exceptions.BusinessException">
    /// request is not Pending (idempotency guard — legacy «Faild Operation» behaviour), or the
    /// proposed guardian's national ID already belongs to another family's provider
    /// («أحد المعيلين مكرر من قبل أكثر من مرة»)
    /// </exception>
    Task<GuardianChangeRequestListDto> DecideRequestAsync(
        Guid requestId,
        ApproveGuardianChangeRequestDto dto,
        string decidedBy,
        string? userRole);
}
