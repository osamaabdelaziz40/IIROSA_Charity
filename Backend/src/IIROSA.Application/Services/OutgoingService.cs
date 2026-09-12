using AutoMapper;
using FluentValidation;
using IIROSA.Application.DTOs.IncomingOutgoing;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Outgoing Letter Service (epic 16, UC-COR-10…19)
///
/// Tenancy mirrors the incoming side: a caller bound to a charity is pinned to it on
/// every read and write; a head-office caller may narrow with the filter's CharityId.
///
/// Serials are allocated per charity + year inside the create transaction (UC-COR-12);
/// Year derives from the letter date. Orphan reports attach under BR-26 (an orphan's
/// report rides at most one letter) and BR-27 (the selection is scoped to the letter's
/// charity) — both refusals surface as «Operation Faild» before any index fires.
/// </summary>
public class OutgoingService : IOutgoingService
{
    private readonly IOutgoingRepository _outgoingRepository;
    private readonly IOutgoingOrphanReportRepository _orphanReportRepository;
    private readonly IOutgoingCategoryRepository _categoryRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IIncomingRepository _incomingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<OutgoingService> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateOutgoingDto> _createValidator;
    private readonly IValidator<UpdateOutgoingDto> _updateValidator;

    public OutgoingService(
        IOutgoingRepository outgoingRepository,
        IOutgoingOrphanReportRepository orphanReportRepository,
        IOutgoingCategoryRepository categoryRepository,
        IDepartmentRepository departmentRepository,
        IIncomingRepository incomingRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<OutgoingService> logger,
        ICurrentUserService currentUser,
        IValidator<CreateOutgoingDto> createValidator,
        IValidator<UpdateOutgoingDto> updateValidator)
    {
        _outgoingRepository = outgoingRepository;
        _orphanReportRepository = orphanReportRepository;
        _categoryRepository = categoryRepository;
        _departmentRepository = departmentRepository;
        _incomingRepository = incomingRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ========== Reads ==========

    /// <summary>Detail read (UC-COR-14). An out-of-scope id returns null, exactly like a missing one.</summary>
    public async Task<OutgoingDto?> GetByIdAsync(Guid id)
    {
        var outgoing = await _outgoingRepository.GetWithDetailsAsync(id);
        if (outgoing == null)
        {
            _logger.LogWarning("Outgoing letter {OutgoingId} not found", id);
            return null;
        }

        if (!IsWithinCallerScope(outgoing))
        {
            _logger.LogWarning("Outgoing letter {OutgoingId} is outside the caller's charity scope", id);
            return null;
        }

        return _mapper.Map<OutgoingDto>(outgoing);
    }

    /// <summary>The §21.S.4 register read (UC-COR-10 / UC-COR-11) — scope-pinned before it reaches the repository.</summary>
    public async Task<(IEnumerable<OutgoingListDto> Items, int TotalCount, int Page)> GetPagedAsync(OutgoingFilterDto filter)
    {
        filter ??= new OutgoingFilterDto();

        ApplyCallerScope(filter);

        var criteria = new OutgoingFilterCriteria
        {
            SearchTerm = filter.SearchTerm,
            Serial = filter.Serial,
            DepartmentId = filter.DepartmentId,
            CategoryId = filter.CategoryId,
            Year = filter.Year,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate,
            HasReply = filter.HasReply,
            CharityId = filter.CharityId,
            // Country dimension of tenancy (review P7): pinned from the caller's claims
            // only — never a client assertion — mirroring MissionService.ApplyCallerScope
            CountryId = _currentUser.CountryId,
            SortBy = filter.SortBy,
            SortOrder = filter.SortOrder
        };

        var (items, totalCount) = await _outgoingRepository.GetPagedAsync(criteria, filter.PageNumber, filter.PageSize);
        var dtos = _mapper.Map<List<OutgoingListDto>>(items);

        return (dtos, totalCount, filter.PageNumber);
    }

    /// <summary>
    /// Register statistics for the band above the §21.S.4 grid (UC-COR-10).
    /// </summary>
    /// <remarks>
    /// Deliberately not filter-reactive: the band describes the caller's whole register
    /// scope, not the current search. The scope rides the same ladder as the register read
    /// — <see cref="ApplyCallerScope(OutgoingFilterDto)"/> over a blank filter plus the
    /// caller's country claim — so the band and the grid beneath it can never disagree
    /// about what is counted.
    /// </remarks>
    public async Task<OutgoingStatisticsDto> GetStatisticsAsync()
    {
        _logger.LogInformation("Getting outgoing letter register statistics");

        // Blank filter ⇒ only the scope pins apply: the caller's charity claim (pinned)
        // and the country claim (criteria) — the register read's first half, unchanged.
        var filter = new OutgoingFilterDto();
        ApplyCallerScope(filter);

        var criteria = new OutgoingFilterCriteria
        {
            CharityId = filter.CharityId,
            // Country dimension of tenancy (review P7): pinned from the caller's claims
            // only — never a client assertion — mirroring MissionService.ApplyCallerScope
            CountryId = _currentUser.CountryId
        };

        var (total, thisYear, addedThisMonth) = await _outgoingRepository.GetRegisterStatisticsAsync(criteria);

        return new OutgoingStatisticsDto
        {
            Total = total,
            ThisYear = thisYear,
            AddedThisMonth = addedThisMonth
        };
    }

    // ========== Writes ==========

    /// <summary>
    /// Register an outgoing letter (UC-COR-13 / §21.S.5). The serial is re-derived inside
    /// this transaction — per charity + year — and Year derives from the letter date.
    /// </summary>
    public async Task<OutgoingDto> CreateAsync(CreateOutgoingDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        await EnsureReferencesExistAsync(dto.DepartmentId, dto.OutgoingCategoryId, dto.IncomingId);

        var year = dto.Date!.Value.Year;
        // A caller with no charity claim creates an HQ-owned (NULL-charity) letter —
        // deliberate D1 decision (رئاسة المكتب correspondence); the orphan pool offered
        // to such letters is deliberately all charities' orphans (BR-27 charity pinning
        // applies to charity-owned letters).
        var charityId = _currentUser.CharityId;

        var serial = await _outgoingRepository.GetNextSerialAsync(charityId, year);
        var serialTxt = serial.ToString("D4");

        var outgoing = new Outgoing
        {
            Serial = serial,
            OutGoingId = serialTxt, // legacy NOT NULL display column — carries the plain serial text
            Subject = dto.Subject.Trim(),
            Date = dto.Date,
            Year = year,
            Fk_DepartmentId = dto.DepartmentId,
            OutgoingCategoryId = dto.OutgoingCategoryId,
            IncomingId = dto.IncomingId,
            UploadedFileId = dto.UploadedFileId,
            FK_CharityId = charityId
        };

        await _outgoingRepository.AddAsync(outgoing);
        try
        {
            await _unitOfWork.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            // Serial race backstop (review P6): two concurrent creates can read the same
            // Max and collide on the unique filtered index — re-derive once inside this
            // transaction and retry; anything else rethrows.
            _logger.LogWarning(
                ex, "Serial collision while registering an outgoing letter for charity {CharityId} year {Year}; re-deriving",
                charityId, year);

            var retrySerial = await _outgoingRepository.GetNextSerialAsync(charityId, year);
            outgoing.Serial = retrySerial;
            outgoing.OutGoingId = retrySerial.ToString("D4");

            _outgoingRepository.Update(outgoing);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Outgoing letter {OutgoingId} registered with serial {Serial} by {UserId}",
            outgoing.Id, serialTxt, _currentUser.UserId);

        return (await GetByIdAsync(outgoing.Id))!;
    }

    /// <summary>
    /// Update an outgoing letter (UC-COR-15). The serial, its year and the charity
    /// ownership are immutable — they anchor the per-charity sequence.
    /// </summary>
    public async Task<OutgoingDto> UpdateAsync(Guid id, UpdateOutgoingDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);
        await EnsureReferencesExistAsync(dto.DepartmentId, dto.OutgoingCategoryId, dto.IncomingId);

        var outgoing = await _outgoingRepository.GetWithDetailsAsync(id);
        if (outgoing == null || !IsWithinCallerScope(outgoing))
        {
            throw new InvalidOperationException($"Outgoing letter {id} not found");
        }

        outgoing.Subject = dto.Subject.Trim();
        outgoing.Date = dto.Date;
        outgoing.Fk_DepartmentId = dto.DepartmentId;
        outgoing.OutgoingCategoryId = dto.OutgoingCategoryId;
        outgoing.IncomingId = dto.IncomingId;
        outgoing.UploadedFileId = dto.UploadedFileId;

        _outgoingRepository.Update(outgoing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Outgoing letter {OutgoingId} updated by {UserId}", id, _currentUser.UserId);

        return (await GetByIdAsync(id))!;
    }

    /// <summary>
    /// Delete an outgoing letter (UC-COR-16) — soft delete, refused while the letter still
    /// has attached orphan reports, per the spec's guard. (Incoming letters no longer link
    /// to outgoing letters, so the incoming-replies guard is structurally always empty.)
    /// </summary>
    public async Task DeleteAsync(Guid id, Guid? deletedBy)
    {
        var outgoing = await _outgoingRepository.GetWithDetailsAsync(id);
        if (outgoing == null || !IsWithinCallerScope(outgoing))
        {
            throw new InvalidOperationException($"Outgoing letter {id} not found");
        }

        var attachedReports = await _orphanReportRepository.GetByOutgoingAsync(id);
        if (attachedReports.Any())
        {
            throw new InvalidOperationException(
                "Operation Faild: the letter carries orphan reports and cannot be deleted");
        }

        outgoing.IsDeleted = true;
        outgoing.DeletedOn = DateTime.UtcNow;
        outgoing.DeletedBy = deletedBy?.ToString() ?? "System";

        _outgoingRepository.Update(outgoing);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Outgoing letter {OutgoingId} deleted by {DeletedBy}", id, outgoing.DeletedBy);
    }

    // ========== Catalogues + serials ==========

    /// <summary>The §21.S.5 category options (UC-COR-17) — table-backed, bilingual.</summary>
    public async Task<IEnumerable<OutgoingCategoryOptionDto>> GetAvailableCategoriesAsync()
    {
        var categories = await _categoryRepository.GetActiveAsync();

        return categories.Select(c => new OutgoingCategoryOptionDto
        {
            Id = c.Id,
            NameAr = c.NameAr,
            NameEn = c.NameEn
        });
    }

    /// <summary>
    /// The advisory next serial (UC-COR-12). The definitive serial is re-derived inside the
    /// create transaction — this read only powers the read-only form field.
    /// </summary>
    public async Task<NextSerialDto> GetNextSerialAsync(int? year, Guid? charityId)
    {
        // Pin, never widen: a caller bound to a charity reads their own sequence — the
        // charityId query parameter is the HQ caller's narrow, never a client assertion.
        var effectiveCharity = _currentUser.CharityId ?? charityId;
        var effectiveYear = year ?? DateTime.UtcNow.Year;

        var next = await _outgoingRepository.GetNextSerialAsync(effectiveCharity, effectiveYear);

        return new NextSerialDto { Serial = next, SerialTxt = next.ToString("D4") };
    }

    // ========== Orphan report attachment (UC-COR-18) ==========

    /// <summary>
    /// The two grids of the §21.S.6 screen: orphans attached to the letter and the letter's
    /// charity's orphans not attached to any letter (BR-26 + BR-27).
    /// </summary>
    public async Task<OutgoingOrphansDto> GetOrphansAsync(Guid outgoingId)
    {
        var outgoing = await _outgoingRepository.GetWithDetailsAsync(outgoingId);
        if (outgoing == null || !IsWithinCallerScope(outgoing))
        {
            throw new InvalidOperationException($"Outgoing letter {outgoingId} not found");
        }

        var links = await _orphanReportRepository.GetByOutgoingAsync(outgoingId);
        var unattached = await _orphanReportRepository.GetUnattachedOrphansAsync(outgoing.FK_CharityId);

        return new OutgoingOrphansDto
        {
            Attached = links.Select(ToOrphanOption).ToList(),
            Unattached = unattached.Select(ToOrphanOption).ToList()
        };
    }

    /// <summary>
    /// Attach an orphan report (UC-COR-18). BR-26: the orphan must not ride another letter;
    /// BR-27: the orphan must belong to the letter's charity. The unattached list encodes
    /// both — membership is the gate.
    /// </summary>
    public async Task AttachOrphanAsync(Guid outgoingId, Guid orphanId)
    {
        var outgoing = await _outgoingRepository.GetWithDetailsAsync(outgoingId);
        if (outgoing == null || !IsWithinCallerScope(outgoing))
        {
            throw new InvalidOperationException($"Outgoing letter {outgoingId} not found");
        }

        if (await _orphanReportRepository.IsOrphanAttachedAsync(orphanId))
        {
            throw new InvalidOperationException(
                "Operation Faild: the orphan's report is already attached to another letter");
        }

        var unattached = await _orphanReportRepository.GetUnattachedOrphansAsync(outgoing.FK_CharityId);
        if (!unattached.Any(o => o.OrphanId == orphanId))
        {
            throw new InvalidOperationException(
                "Operation Faild: the orphan is outside the letter's charity selection");
        }

        await _orphanReportRepository.AddAsync(new OutgoingOrphanReport { OutgoingId = outgoingId, OrphanId = orphanId });
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Orphan {OrphanId} attached to outgoing letter {OutgoingId} by {Caller}",
            orphanId, outgoingId, _currentUser.UserId);
    }

    /// <summary>Detach an orphan report (UC-COR-18) — soft delete of the link row.</summary>
    public async Task DetachOrphanAsync(Guid outgoingId, Guid orphanId)
    {
        var outgoing = await _outgoingRepository.GetWithDetailsAsync(outgoingId);
        if (outgoing == null || !IsWithinCallerScope(outgoing))
        {
            throw new InvalidOperationException($"Outgoing letter {outgoingId} not found");
        }

        var link = await _orphanReportRepository.FindByLetterAndOrphanAsync(outgoingId, orphanId);
        if (link == null)
        {
            throw new InvalidOperationException("Operation Faild: the orphan is not attached to this letter");
        }

        link.IsDeleted = true;
        link.DeletedOn = DateTime.UtcNow;
        link.DeletedBy = _currentUser.UserId?.ToString() ?? "System";

        _orphanReportRepository.Update(link);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Orphan {OrphanId} detached from outgoing letter {OutgoingId} by {Caller}",
            orphanId, outgoingId, _currentUser.UserId);
    }

    // ========== Orphans-by-outgoing-letter report (UC-COR-19) ==========

    /// <summary>
    /// The §21.S.7 report: outgoing letters carrying the requested orphan's report. The
    /// child code (كود اليتيم) is mandatory per the spec — the refusal names it.
    /// </summary>
    public async Task<(IEnumerable<OutgoingOrphanReportRowDto> Items, int TotalCount, int Page)> GetOrphanReportAsync(
        OutgoingOrphanReportFilterDto filter)
    {
        filter ??= new OutgoingOrphanReportFilterDto();

        if (string.IsNullOrWhiteSpace(filter.ChildCode))
        {
            throw new InvalidOperationException("Operation Faild: orphan code (كود اليتيم) is required");
        }

        ApplyCallerScope(filter);

        var (rows, totalCount) = await _orphanReportRepository.GetOrphanReportAsync(
            filter.Serial,
            filter.Year,
            filter.CharityId,
            _currentUser.CountryId, // country pin from claims only (review P7)
            filter.DateFrom,
            filter.DateTo,
            filter.ChildCode.Trim(),
            filter.PageNumber,
            filter.PageSize);

        var dtos = rows.Select(r => new OutgoingOrphanReportRowDto
        {
            OutgoingId = r.OutgoingId,
            Serial = r.Serial,
            Year = r.Year,
            LetterDate = r.LetterDate,
            CharityName = r.CharityName,
            OrphanCount = r.OrphanCount,
            OrphanAttached = r.OrphanAttached
        }).ToList();

        return (dtos, totalCount, filter.PageNumber);
    }

    // ========== Helpers ==========

    /// <summary>
    /// FK existence checks (§21.S.5 / §21.U.13): a bogus department, category or reply-to
    /// incoming letter is a field-flagging 400 (ValidationException → the controller's
    /// errors map), never an FK-constraint 500. The reply-to letter must also sit inside
    /// the caller's charity when the caller is pinned to one.
    /// </summary>
    private async Task EnsureReferencesExistAsync(int? departmentId, int? categoryId, Guid? incomingId)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        if (departmentId.HasValue && await _departmentRepository.GetByIdAsync(departmentId.Value) == null)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                nameof(CreateOutgoingDto.DepartmentId), "Routing department does not exist"));
        }

        // Existence, not activity (review P10): a letter referencing a category that was
        // later deactivated must stay editable — the drop-downs serve actives only, the
        // FK check accepts any existing row.
        if (categoryId.HasValue && await _categoryRepository.GetByIdAsync(categoryId.Value) == null)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                nameof(CreateOutgoingDto.OutgoingCategoryId), "Outgoing category does not exist"));
        }

        if (incomingId.HasValue)
        {
            var incoming = await _incomingRepository.GetWithDetailsAsync(incomingId.Value);
            if (incoming == null)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(CreateOutgoingDto.IncomingId), "The incoming letter being replied to does not exist"));
            }
            else if (_currentUser.CharityId.HasValue && incoming.FK_CharityId != _currentUser.CharityId.Value)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(CreateOutgoingDto.IncomingId), "The incoming letter being replied to is outside the caller's charity"));
            }
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }

    private static OrphanOptionDto ToOrphanOption(OutgoingOrphanReport link)
    {
        var orphan = link.Orphan!;
        return new OrphanOptionDto
        {
            OrphanId = orphan.Id,
            Code = orphan.Code ?? string.Empty,
            FullName = orphan.FullName ?? string.Empty,
            GuarantorName = orphan.Family?.HeadOfFamily,
            Kinship = orphan.Family?.ProviderType
        };
    }

    private static OrphanOptionDto ToOrphanOption(OrphanCandidateRow row)
    {
        return new OrphanOptionDto
        {
            OrphanId = row.OrphanId,
            Code = row.Code,
            FullName = row.FullName,
            GuarantorName = row.GuarantorName,
            Kinship = row.Kinship
        };
    }

    /// <summary>
    /// Pin the caller's charity claim onto the filter — pin, never widen. A head-office
    /// caller without the claim keeps their explicit CharityId narrow (or sees all).
    /// </summary>
    private void ApplyCallerScope(OutgoingFilterDto filter)
    {
        var callerCharity = _currentUser.CharityId;
        if (!callerCharity.HasValue)
        {
            return;
        }

        if (filter.CharityId.HasValue && filter.CharityId.Value != callerCharity.Value)
        {
            _logger.LogWarning(
                "Caller pinned to charity {CallerCharity} requested charity {RequestedCharity}; pinning to {CallerCharity}",
                callerCharity.Value, filter.CharityId.Value, callerCharity.Value);
        }

        filter.CharityId = callerCharity;
    }

    /// <summary>
    /// Pin the caller's charity claim onto the §21.S.7 report filter (UC-COR-19 overload).
    /// </summary>
    private void ApplyCallerScope(OutgoingOrphanReportFilterDto filter)
    {
        var callerCharity = _currentUser.CharityId;
        if (!callerCharity.HasValue)
        {
            return;
        }

        if (filter.CharityId.HasValue && filter.CharityId.Value != callerCharity.Value)
        {
            _logger.LogWarning(
                "Caller pinned to charity {CallerCharity} requested charity {RequestedCharity}; pinning to {CallerCharity}",
                callerCharity.Value, filter.CharityId.Value, callerCharity.Value);
        }

        filter.CharityId = callerCharity;
    }

    /// <summary>
    /// Whether the letter is visible to the caller: a charity-pinned caller sees only
    /// their own charity's letters; a country-pinned caller (no charity claim) sees the
    /// letters of their country's charities (review P7); a caller with neither claim
    /// sees everything. NULL-charity (HQ-owned) letters belong to no country and no charity.
    /// </summary>
    private bool IsWithinCallerScope(Outgoing outgoing)
    {
        var callerCharity = _currentUser.CharityId;
        if (callerCharity.HasValue)
        {
            return outgoing.FK_CharityId == callerCharity.Value;
        }

        var callerCountry = _currentUser.CountryId;
        if (callerCountry.HasValue)
        {
            return outgoing.Charity != null && outgoing.Charity.CountryId == callerCountry.Value;
        }

        return true;
    }
}
