using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using IIROSA.Application.DTOs;
using IIROSA.Application.DTOs.Family;
using IIROSA.Application.Exceptions;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Enums;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Guardian-change request service (UC-FAM-09 طلبات تعديل العائل).
///
/// The review queue and the raise-producer for guardian changes: a charity proposes a new
/// guardian for one of its families, the head office reviews the old/new snapshots side by side
/// and (in 5-10) approves or rejects. All old-guardian context is snapshotted onto the request at
/// raise time — the queue never joins live family data for those columns.
/// </summary>
public class GuardianChangeRequestService : IGuardianChangeRequestService
{
    // The literal legacy refusal — keep verbatim (UC-FAM-05's pending-duplicate guard).
    private const string DuplicatePendingMessage = "تم اضافه الطلب من قبل . انتظر موافه مسئول المكتب";

    private readonly IGuardianChangeRequestRepository _requestRepository;
    private readonly IFamilyRepository _familyRepository;
    private readonly IOrphanRepository _orphanRepository;
    private readonly IProviderRepository _providerRepository;
    private readonly IFatherRepository _fatherRepository;
    private readonly IMotherRepository _motherRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateGuardianChangeRequestDto> _createValidator;
    private readonly IValidator<ApproveGuardianChangeRequestDto> _approveValidator;
    private readonly IMapper _mapper;
    private readonly ILogger<GuardianChangeRequestService> _logger;

    public GuardianChangeRequestService(
        IGuardianChangeRequestRepository requestRepository,
        IFamilyRepository familyRepository,
        IOrphanRepository orphanRepository,
        IProviderRepository providerRepository,
        IFatherRepository fatherRepository,
        IMotherRepository motherRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateGuardianChangeRequestDto> createValidator,
        IValidator<ApproveGuardianChangeRequestDto> approveValidator,
        IMapper mapper,
        ILogger<GuardianChangeRequestService> logger)
    {
        _requestRepository = requestRepository;
        _familyRepository = familyRepository;
        _orphanRepository = orphanRepository;
        _providerRepository = providerRepository;
        _fatherRepository = fatherRepository;
        _motherRepository = motherRepository;
        _unitOfWork = unitOfWork;
        _createValidator = createValidator;
        _approveValidator = approveValidator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PagedResult<GuardianChangeRequestListDto>> GetRequestsAsync(
        GuardianChangeRequestFilterDto filter,
        Guid? userCharityId,
        string? userRole)
    {
        var pageNumber = Math.Max(1, filter.PageNumber ?? 1);
        var pageSize = Math.Clamp(filter.PageSize ?? 10, 1, 100);

        var query = _requestRepository.TableNoTracking
            .Where(r => !r.IsDeleted);

        // Tenancy: a Charity-role caller only ever sees its own requests, whatever it sends.
        // HQ roles see all and may narrow through the charity filter.
        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            var charityId = userCharityId
                ?? throw new UnauthorizedAccessException("No charity is associated with this account");
            query = query.Where(r => r.CharityId == charityId);
        }
        else if (filter.CharityId.HasValue)
        {
            query = query.Where(r => r.CharityId == filter.CharityId.Value);
        }

        // The queue opens on the pending requests; history views pass the other statuses.
        // A numeric-but-undefined value (e.g. ?status=0) binds fine and would otherwise filter
        // the queue to a silently empty grid — refuse it as a bad filter instead.
        if (filter.Status.HasValue && !Enum.IsDefined(filter.Status.Value))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(filter.Status),
                    "Status must be a defined request status (1=Pending, 2=Approved, 3=Rejected)")
            });
        }
        var status = filter.Status ?? GuardianChangeRequestStatus.Pending;
        query = query.Where(r => r.Status == status);

        var totalCount = await query.CountAsync();

        var rows = await query
            .OrderByDescending(r => r.CreatedOn)
            // Tiebreaker: same-second rows (bulk raise, double-click) have undefined SQL Server
            // order without it — paging could repeat a row on two pages and skip another.
            .ThenBy(r => r.Id)
            .Include(r => r.Charity)
            // The queue's FamilyCode column (the reviewer's family identification in the
            // confirm modal) reads r.Family.Code — without the include it is null for every row.
            .Include(r => r.Family)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<GuardianChangeRequestListDto>
        {
            Items = _mapper.Map<IEnumerable<GuardianChangeRequestListDto>>(rows),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public async Task<GuardianChangeRequestListDto> CreateRequestAsync(
        Guid familyId,
        CreateGuardianChangeRequestDto dto,
        string requestedByName,
        Guid? userCharityId,
        string? userRole)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var family = await _familyRepository.GetByIdAsync(familyId);
        if (family is null || family.IsDeleted)
        {
            throw new NotFoundException(typeof(Family), familyId);
        }

        // The raising charity is stamped from the family file — never from the payload.
        var charityId = family.FK_CharityId ?? family.CharityId
            ?? throw new BusinessException("This family is not linked to a charity, so its guardian-change request cannot be raised");

        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            if (userCharityId is null || userCharityId != charityId)
            {
                throw new UnauthorizedAccessException("This family belongs to another charity");
            }
        }

        // The legacy pending-duplicate guard — one open request per family at a time.
        var hasPending = await _requestRepository.TableNoTracking
            .AnyAsync(r => r.FamilyId == familyId && r.Status == GuardianChangeRequestStatus.Pending && !r.IsDeleted);
        if (hasPending)
        {
            throw new BusinessException(DuplicatePendingMessage);
        }

        // Old-guardian + mother snapshots straight from the family file at raise time.
        // The guardian seat comes from the provider repository, NOT from
        // IncludeNavigationProperties — that method never loads Family.Provider, so reading the
        // snapshot off it stored null for every request and the reviewer approved blind.
        var familyWithGuardians = await _familyRepository
            .IncludeNavigationProperties()
            .FirstOrDefaultAsync(f => f.Id == familyId && !f.IsDeleted);
        var oldProvider = await _providerRepository.GetByFamilyIdAsync(familyId);

        // Parent-designated families (guardian of record = Father/Mother flag, no Provider row —
        // 5-13's ruling made that designation a first-class guardian state): snapshot the
        // designated parent instead, so the reviewer sees the real acting guardian rather than
        // blank columns. The relationship snapshot carries the provider's declared relationship,
        // or the designation itself for the parent shape (DoD §10.U.09, 2026-08-24).
        string? oldGuardianName = oldProvider?.FullName;
        string? oldGuardianNationalId = oldProvider?.NationalId;
        string? oldGuardianRelationship = oldProvider?.RelationshipToFamily;
        if (oldProvider is null)
        {
            if (string.Equals(family.ProviderType, "Father", StringComparison.OrdinalIgnoreCase))
            {
                oldGuardianName = familyWithGuardians?.Father?.FullName;
                oldGuardianNationalId = familyWithGuardians?.Father?.NationalId;
                oldGuardianRelationship = "Father";
            }
            else if (string.Equals(family.ProviderType, "Mother", StringComparison.OrdinalIgnoreCase))
            {
                oldGuardianName = familyWithGuardians?.Mother?.FullName;
                oldGuardianNationalId = familyWithGuardians?.Mother?.NationalId;
                oldGuardianRelationship = "Mother";
            }
        }

        // Oldest orphan for the queue's context columns (كود اليتيم / اسم اليتيم كامل).
        var firstOrphan = await _orphanRepository.TableNoTracking
            .Where(o => o.FamilyId == familyId && !o.IsDeleted)
            .OrderBy(o => o.CreatedOn)
            .FirstOrDefaultAsync();

        var request = new GuardianChangeRequest
        {
            FamilyId = familyId,
            CharityId = charityId,
            OrphanCode = firstOrphan?.Code,
            OrphanName = firstOrphan?.FullName,
            MotherName = familyWithGuardians?.Mother?.FullName,
            OldGuardianName = oldGuardianName,
            OldGuardianNationalId = oldGuardianNationalId,
            OldGuardianRelationship = oldGuardianRelationship,
            NewGuardianName = dto.NewGuardianName.Trim(),
            NewGuardianNationalId = dto.NewGuardianNationalId.Trim(),
            Relationship = dto.Relationship.Trim(),
            Reason = dto.Reason.Trim(),
            RequestedByName = string.IsNullOrWhiteSpace(requestedByName) ? "Unknown" : requestedByName,
            Status = GuardianChangeRequestStatus.Pending
        };

        await _requestRepository.InsertAsync(request);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Guardian-change request {RequestId} raised for family {FamilyId} by {User} (charity {CharityId})",
            request.Id, familyId, requestedByName, charityId);

        // Re-read through the queue projection so the caller gets the charity name too.
        var created = await _requestRepository.TableNoTracking
            .Where(r => r.Id == request.Id)
            .Include(r => r.Charity)
            .FirstOrDefaultAsync();

        return _mapper.Map<GuardianChangeRequestListDto>(created ?? request);
    }

    /// <inheritdoc />
    public async Task<GuardianChangeRequestListDto> DecideRequestAsync(
        Guid requestId,
        ApproveGuardianChangeRequestDto dto,
        string decidedBy,
        string? userRole)
    {
        await _approveValidator.ValidateAndThrowAsync(dto);

        // The endpoint authorises HQ only; the service re-refuses a charity-role caller —
        // defense in depth, the platform's standing rule (same as member control / 5-13).
        if (string.Equals(userRole, "Charity", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Only the head office may decide guardian-change requests");
        }

        var request = await _requestRepository.Table
            .Include(r => r.Charity)
            .Include(r => r.Family)
            .FirstOrDefaultAsync(r => r.Id == requestId && !r.IsDeleted)
            ?? throw new NotFoundException(typeof(GuardianChangeRequest), requestId);

        // Idempotency guard — the legacy «Faild Operation»: a decided request never moves again.
        if (request.Status != GuardianChangeRequestStatus.Pending)
        {
            throw new BusinessException("This request has already been decided");
        }

        // A request whose family was deleted while it pended must never be APPLIED — the seat
        // it targets is no longer on the live register. Refusing stays available for cleanup.
        if (dto.IsApproved && (request.Family is null || request.Family.IsDeleted))
        {
            throw new BusinessException("The family this request concerns no longer exists; refuse the request instead of approving it");
        }

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            // In-transaction re-check: the load above ran before the transaction opened, so a
            // decision committed inside that window would otherwise be clobbered here
            // (no concurrency token exists on this entity — see deferred-work).
            var stillPending = await _requestRepository.TableNoTracking.AnyAsync(
                r => r.Id == requestId && r.Status == GuardianChangeRequestStatus.Pending);
            if (!stillPending)
            {
                throw new BusinessException("This request has already been decided");
            }

            var decider = string.IsNullOrWhiteSpace(decidedBy) ? "Unknown" : decidedBy;

            if (dto.IsApproved)
            {
                await ApplyGuardianToFamilyAsync(request);

                // The create-time pending-duplicate guard is check-then-insert, so a concurrent
                // double-raise can leave a sibling pending behind. Void those here, inside the
                // same transaction — a surviving sibling would re-apply a stale snapshot over
                // the guardian this decision just installed.
                var siblings = await _requestRepository.Table
                    .Where(r => r.FamilyId == request.FamilyId
                        && r.Id != request.Id
                        && r.Status == GuardianChangeRequestStatus.Pending
                        && !r.IsDeleted)
                    .ToListAsync();
                foreach (var sibling in siblings)
                {
                    sibling.Status = GuardianChangeRequestStatus.Rejected;
                    sibling.RejectionReason = "Superseded by an approved request for the same family";
                    sibling.DecidedBy = decider;
                    sibling.DecidedOn = DateTime.UtcNow;
                }

                request.Status = GuardianChangeRequestStatus.Approved;
            }
            else
            {
                request.RejectionReason = dto.RejectionReason!.Trim();
                request.Status = GuardianChangeRequestStatus.Rejected;
            }

            request.DecidedBy = decider;
            request.DecidedOn = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }

        _logger.LogInformation(
            "Guardian-change request {RequestId} decided {Status} by {User}",
            requestId, request.Status, decidedBy);

        return _mapper.Map<GuardianChangeRequestListDto>(request);
    }

    /// <summary>
    /// Apply the approved snapshot to the family's guardian seat: the family's current provider
    /// row is updated in place (name, national ID, relationship) — or created when the family has
    /// no provider row. Orphans and sponsorship are never touched by a guardian change.
    /// </summary>
    private async Task ApplyGuardianToFamilyAsync(GuardianChangeRequest request)
    {
        var provider = await _providerRepository.GetByFamilyIdAsync(request.FamilyId);
        var family = request.Family
            ?? throw new BusinessException("The family this request concerns no longer exists");

        // Stale-snapshot guard: the raise→decide window is days long, and member control,
        // 5-13's remove-link and the refugee edits all write this same seat. If the live
        // guardian no longer matches what the request was raised against, applying it would
        // silently clobber the newer record — refuse and re-raise instead. An EMPTY seat
        // (provider removed via 5-13 while the request pended) is a legitimate re-attach.
        if (provider is not null
            && !string.Equals(provider.NationalId, request.OldGuardianNationalId, StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("The family's guardian changed after this request was raised; refuse it and raise a new request against the current guardian");
        }

        // UC-REF-03 «أحد المعيلين مكرر من قبل أكثر من مرة» — one provider national ID per family;
        // exclude the row being replaced so an in-place update is always allowed.
        if (await _providerRepository.IsNationalIdExistsAsync(
                request.NewGuardianNationalId, excludeId: provider?.Id))
        {
            throw new BusinessException("أحد المعيلين مكرر من قبل أكثر من مرة");
        }

        if (provider is not null)
        {
            provider.FullName = request.NewGuardianName;
            provider.NationalId = request.NewGuardianNationalId;
            provider.RelationshipToFamily = request.Relationship;
        }
        else
        {
            provider = new Provider
            {
                Id = Guid.NewGuid(),
                FamilyId = request.FamilyId,
                FullName = request.NewGuardianName,
                NationalId = request.NewGuardianNationalId,
                RelationshipToFamily = request.Relationship,
                Phone = string.Empty
            };
            await _providerRepository.InsertAsync(provider);

            // Every other provider-add path stamps the designation (FamilyService precedent);
            // leaving a stale Father/Mother ProviderType beside a distinct Provider row
            // contradicts BR-06's one-guardian-of-record rule.
            var outgoingDesignation = family.ProviderType;
            family.ProviderType = "Other";

            // 5-13's ruling made the parent designation a first-class guardian seat, so approving
            // a distinct provider on a parent-designated family VACATES that seat: the parent's
            // IsProvider flag goes with the designation, or the mother lingers on 5-14's widow
            // sheet as a guardian of record she no longer is (same clearing as remove-link).
            if (string.Equals(outgoingDesignation, "Father", StringComparison.OrdinalIgnoreCase))
            {
                var father = await _fatherRepository.GetByFamilyIdAsync(request.FamilyId);
                if (father is not null)
                {
                    father.IsProvider = false;
                }
            }
            else if (string.Equals(outgoingDesignation, "Mother", StringComparison.OrdinalIgnoreCase))
            {
                var mother = await _motherRepository.GetByFamilyIdAsync(request.FamilyId);
                if (mother is not null)
                {
                    mother.IsProvider = false;
                }
            }
        }

        // Head-of-family mirror — same as every provider-add path in FamilyService.
        family.HeadOfFamily = request.NewGuardianName;
    }
}
