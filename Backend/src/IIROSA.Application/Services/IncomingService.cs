using AutoMapper;
using FluentValidation;
using Framework.Identity.Data.Services.Interfaces;
using IIROSA.Application.DTOs.IncomingOutgoing;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// Incoming Letter Service (epic 16, UC-COR-01…09)
///
/// Tenancy: a caller bound to a charity is pinned to it on every read and write — pin,
/// never widen — and the single-record paths treat an out-of-scope row exactly like a
/// missing one. A head-office caller without a charity claim sees everything and may
/// narrow with the filter's CharityId.
///
/// Serials are allocated per charity + year inside the create transaction (UC-COR-03);
/// the GET endpoint is advisory only. Only the UnitOfWork saves; validators run in this
/// layer per the platform rule.
/// </summary>
public class IncomingService : IIncomingService
{
    private const string DefaultStatus = "معلق";

    // The spec's tri-state (§21.S.1): the stored value is the Arabic term.
    private static readonly (string Id, string NameAr, string NameEn, string Color)[] Statuses =
    {
        ("معلق", "معلق", "Pending", "warning"),
        ("تم الرد", "تم الرد", "Replied", "success"),
        ("تم عمل اللازم", "تم عمل اللازم", "Actioned", "secondary")
    };

    private readonly IIncomingRepository _incomingRepository;
    private readonly IIncomingEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUserAppServiceExtended _userAppService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<IncomingService> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<CreateIncomingDto> _createValidator;
    private readonly IValidator<UpdateIncomingDto> _updateValidator;

    public IncomingService(
        IIncomingRepository incomingRepository,
        IIncomingEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IUserAppServiceExtended userAppService,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<IncomingService> logger,
        ICurrentUserService currentUser,
        IValidator<CreateIncomingDto> createValidator,
        IValidator<UpdateIncomingDto> updateValidator)
    {
        _incomingRepository = incomingRepository;
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _userAppService = userAppService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _currentUser = currentUser;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ========== Reads ==========

    /// <summary>Detail read (UC-COR-05). An out-of-scope id returns null, exactly like a missing one.</summary>
    public async Task<IncomingDto?> GetByIdAsync(Guid id)
    {
        var incoming = await _incomingRepository.GetWithDetailsAsync(id);
        if (incoming == null)
        {
            _logger.LogWarning("Incoming letter {IncomingId} not found", id);
            return null;
        }

        if (!IsWithinCallerScope(incoming))
        {
            _logger.LogWarning("Incoming letter {IncomingId} is outside the caller's charity scope", id);
            return null;
        }

        return _mapper.Map<IncomingDto>(incoming);
    }

    /// <summary>The §21.S.1 register read (UC-COR-01 / UC-COR-02) — scope-pinned before it reaches the repository.</summary>
    public async Task<(IEnumerable<IncomingListDto> Items, int TotalCount, int Page)> GetPagedAsync(IncomingFilterDto filter)
    {
        filter ??= new IncomingFilterDto();

        ApplyCallerScope(filter);

        var criteria = new IncomingFilterCriteria
        {
            SearchTerm = filter.SearchTerm,
            Serial = filter.Serial,
            LetterNumber = filter.LetterNumber,
            DepartmentId = filter.DepartmentId,
            Status = filter.Status,
            Year = filter.Year,
            StartDate = filter.StartDate,
            EndDate = filter.EndDate,
            AssignedUserId = filter.AssignedUserId,
            CharityId = filter.CharityId,
            // Country dimension of tenancy (review P7): pinned from the caller's claims
            // only — never a client assertion — mirroring MissionService.ApplyCallerScope
            CountryId = _currentUser.CountryId,
            SortBy = filter.SortBy,
            SortOrder = filter.SortOrder
        };

        var (items, totalCount) = await _incomingRepository.GetPagedAsync(criteria, filter.PageNumber, filter.PageSize);
        var dtos = _mapper.Map<List<IncomingListDto>>(items);

        return (dtos, totalCount, filter.PageNumber);
    }

    /// <summary>
    /// Register statistics for the band above the §21.S.1 grid (UC-COR-01).
    /// </summary>
    /// <remarks>
    /// Deliberately not filter-reactive: the band describes the caller's whole register
    /// scope, not the current search. The scope rides the same ladder as the register read
    /// — <see cref="ApplyCallerScope(IncomingFilterDto)"/> over a blank filter plus the
    /// caller's country claim — so the band and the grid beneath it can never disagree
    /// about what is counted.
    /// </remarks>
    public async Task<IncomingStatisticsDto> GetStatisticsAsync()
    {
        _logger.LogInformation("Getting incoming letter register statistics");

        // Blank filter ⇒ only the scope pins apply: the caller's charity claim (pinned)
        // and the country claim (criteria) — the register read's first half, unchanged.
        var filter = new IncomingFilterDto();
        ApplyCallerScope(filter);

        var criteria = new IncomingFilterCriteria
        {
            CharityId = filter.CharityId,
            // Country dimension of tenancy (review P7): pinned from the caller's claims
            // only — never a client assertion — mirroring MissionService.ApplyCallerScope
            CountryId = _currentUser.CountryId
        };

        var (total, thisYear, addedThisMonth) = await _incomingRepository.GetRegisterStatisticsAsync(criteria);

        return new IncomingStatisticsDto
        {
            Total = total,
            ThisYear = thisYear,
            AddedThisMonth = addedThisMonth
        };
    }

    // ========== Writes ==========

    /// <summary>
    /// Register an incoming letter (UC-COR-04 / §21.S.2). The serial is re-derived inside
    /// this transaction — per charity + year — so the advisory GET can never collide.
    /// </summary>
    public async Task<IncomingDto> CreateAsync(CreateIncomingDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);
        await EnsureReferencesExistAsync(dto.DepartmentId, dto.AssignedUserId);

        var year = dto.Date!.Value.Year;
        // A caller with no charity claim creates an HQ-owned (NULL-charity) letter —
        // deliberate D1 decision (رئاسة المكتب correspondence); scoping, uniqueness and
        // the orphan pool treat NULL as its own scope, never as "every charity".
        var charityId = _currentUser.CharityId;

        if (!string.IsNullOrWhiteSpace(dto.LetterNumber) &&
            !await _incomingRepository.IsLetterNumberUniqueAsync(dto.LetterNumber.Trim(), charityId, year))
        {
            throw new InvalidOperationException(
                "Operation Faild: this letter number is already registered for the charity and year");
        }

        EnsureKnownStatus(dto.Status);

        var serial = await _incomingRepository.GetNextSerialAsync(charityId, year);
        var serialTxt = serial.ToString("D4");

        var incoming = new Incoming
        {
            Serial = serial,
            Serial_Txt = serialTxt,
            IncomingId = serialTxt, // legacy NOT NULL display column — carries the plain serial text
            Subject = dto.Subject.Trim(),
            Date = dto.Date,
            LetterNumber = string.IsNullOrWhiteSpace(dto.LetterNumber) ? null : dto.LetterNumber.Trim(),
            LetterDate = dto.LetterDate,
            Year = year,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? DefaultStatus : dto.Status.Trim(),
            LetterDescription = dto.LetterDescription,
            FK_DepartmentId = dto.DepartmentId,
            FK_UserId = dto.AssignedUserId,
            UploadedFileId = dto.UploadedFileId,
            FK_CharityId = charityId
        };

        await _incomingRepository.AddAsync(incoming);
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
                ex, "Serial collision while registering an incoming letter for charity {CharityId} year {Year}; re-deriving",
                charityId, year);

            var retrySerial = await _incomingRepository.GetNextSerialAsync(charityId, year);
            incoming.Serial = retrySerial;
            incoming.Serial_Txt = retrySerial.ToString("D4");
            incoming.IncomingId = incoming.Serial_Txt;

            _incomingRepository.Update(incoming);
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation(
            "Incoming letter {IncomingId} registered with serial {Serial} by {UserId}",
            incoming.Id, serialTxt, _currentUser.UserId);

        return (await GetByIdAsync(incoming.Id))!;
    }

    /// <summary>
    /// Update an incoming letter (UC-COR-06). The serial, its year and the charity
    /// ownership are immutable — they anchor the per-charity sequence.
    /// </summary>
    public async Task<IncomingDto> UpdateAsync(Guid id, UpdateIncomingDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);
        await EnsureReferencesExistAsync(dto.DepartmentId, dto.AssignedUserId);

        var incoming = await _incomingRepository.GetWithDetailsAsync(id);
        if (incoming == null || !IsWithinCallerScope(incoming))
        {
            throw new InvalidOperationException($"Incoming letter {id} not found");
        }

        if (!string.IsNullOrWhiteSpace(dto.LetterNumber) &&
            !await _incomingRepository.IsLetterNumberUniqueAsync(
                dto.LetterNumber.Trim(), incoming.FK_CharityId,
                incoming.Year ?? dto.Date!.Value.Year, // legacy rows may carry a NULL year (review P9)
                excludeId: id))
        {
            throw new InvalidOperationException(
                "Operation Faild: this letter number is already registered for the charity and year");
        }

        EnsureKnownStatus(dto.Status);

        incoming.Subject = dto.Subject.Trim();
        incoming.Date = dto.Date;
        incoming.LetterNumber = string.IsNullOrWhiteSpace(dto.LetterNumber) ? null : dto.LetterNumber.Trim();
        incoming.LetterDate = dto.LetterDate;
        incoming.Status = string.IsNullOrWhiteSpace(dto.Status) ? DefaultStatus : dto.Status.Trim();
        incoming.LetterDescription = dto.LetterDescription;
        incoming.FK_DepartmentId = dto.DepartmentId;
        incoming.FK_UserId = dto.AssignedUserId;
        incoming.UploadedFileId = dto.UploadedFileId;

        _incomingRepository.Update(incoming);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Incoming letter {IncomingId} updated by {UserId}", id, _currentUser.UserId);

        return (await GetByIdAsync(id))!;
    }

    /// <summary>
    /// Delete an incoming letter (UC-COR-07) — soft delete, refused while the letter still
    /// has outgoing replies, per the spec's guard.
    /// </summary>
    public async Task DeleteAsync(Guid id, Guid? deletedBy)
    {
        var incoming = await _incomingRepository.GetWithDetailsAsync(id);
        if (incoming == null || !IsWithinCallerScope(incoming))
        {
            throw new InvalidOperationException($"Incoming letter {id} not found");
        }

        var replies = await _incomingRepository.CountOutgoingRepliesAsync(id);
        if (replies > 0)
        {
            throw new InvalidOperationException(
                "Operation Faild: the letter has outgoing replies and cannot be deleted");
        }

        incoming.IsDeleted = true;
        incoming.DeletedOn = DateTime.UtcNow;
        incoming.DeletedBy = deletedBy?.ToString() ?? "System";

        _incomingRepository.Update(incoming);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Incoming letter {IncomingId} deleted by {DeletedBy}", id, incoming.DeletedBy);
    }

    // ========== Catalogues + serials ==========

    /// <summary>The §21.S.1 status options (UC-COR-01) — tri-state, Arabic-stored.</summary>
    public Task<IEnumerable<CorrespondenceStatusDto>> GetAvailableStatusesAsync()
    {
        var statuses = Statuses.Select(s => new CorrespondenceStatusDto
        {
            Id = s.Id,
            NameAr = s.NameAr,
            NameEn = s.NameEn,
            Color = s.Color
        });

        return Task.FromResult<IEnumerable<CorrespondenceStatusDto>>(statuses);
    }

    /// <summary>
    /// The advisory next serial (UC-COR-03). The definitive serial is re-derived inside the
    /// create transaction — this read only powers the read-only form field.
    /// </summary>
    public async Task<NextSerialDto> GetNextSerialAsync(int? year, Guid? charityId)
    {
        // Pin, never widen: a caller bound to a charity reads their own sequence — the
        // charityId query parameter is the HQ caller's narrow, never a client assertion.
        var effectiveCharity = _currentUser.CharityId ?? charityId;
        var effectiveYear = year ?? DateTime.UtcNow.Year;

        var next = await _incomingRepository.GetNextSerialAsync(effectiveCharity, effectiveYear);

        return new NextSerialDto { Serial = next, SerialTxt = next.ToString("D4") };
    }

    // ========== Employee attachment (UC-COR-09) ==========

    /// <summary>The two lists of the §21.S.3 screen: attached employees and available ones.</summary>
    public async Task<IncomingEmployeesDto> GetEmployeesAsync(Guid incomingId)
    {
        var incoming = await _incomingRepository.GetWithDetailsAsync(incomingId);
        if (incoming == null || !IsWithinCallerScope(incoming))
        {
            throw new InvalidOperationException($"Incoming letter {incomingId} not found");
        }

        var links = await _employeeRepository.GetByIncomingAsync(incomingId);
        var available = await _employeeRepository.GetAvailableEmployeesAsync(incomingId, incoming.FK_CharityId);

        return new IncomingEmployeesDto
        {
            Attached = links.Select(l => new EmployeeOptionDto
            {
                UserId = l.UserId,
                FullName = l.User?.FullName ?? string.Empty,
                Email = l.User?.Email
            }).ToList(),
            Available = available.Select(a => new EmployeeOptionDto
            {
                UserId = a.UserId,
                FullName = a.FullName,
                Email = a.Email
            }).ToList()
        };
    }

    /// <summary>Attach an employee (UC-COR-09) — refused with «Operation Faild» on a duplicate or unknown link.</summary>
    public async Task AttachEmployeeAsync(Guid incomingId, Guid userId)
    {
        var incoming = await _incomingRepository.GetWithDetailsAsync(incomingId);
        if (incoming == null || !IsWithinCallerScope(incoming))
        {
            throw new InvalidOperationException($"Incoming letter {incomingId} not found");
        }

        if (await _employeeRepository.ExistsAsync(incomingId, userId))
        {
            throw new InvalidOperationException("Operation Faild: the employee is already attached to this letter");
        }

        var available = await _employeeRepository.GetAvailableEmployeesAsync(incomingId, incoming.FK_CharityId);
        if (!available.Any(u => u.UserId == userId))
        {
            throw new InvalidOperationException("Operation Faild: the employee is unknown or inactive");
        }

        await _employeeRepository.AddAsync(new IncomingEmployee { IncomingId = incomingId, UserId = userId });
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Employee {UserId} attached to incoming letter {IncomingId} by {Caller}",
            userId, incomingId, _currentUser.UserId);
    }

    /// <summary>Detach an employee (UC-COR-09) — soft delete of the link row.</summary>
    public async Task DetachEmployeeAsync(Guid incomingId, Guid userId)
    {
        var incoming = await _incomingRepository.GetWithDetailsAsync(incomingId);
        if (incoming == null || !IsWithinCallerScope(incoming))
        {
            throw new InvalidOperationException($"Incoming letter {incomingId} not found");
        }

        var links = await _employeeRepository.GetByIncomingAsync(incomingId);
        var link = links.FirstOrDefault(l => l.UserId == userId);
        if (link == null)
        {
            throw new InvalidOperationException("Operation Faild: the employee is not attached to this letter");
        }

        link.IsDeleted = true;
        link.DeletedOn = DateTime.UtcNow;
        link.DeletedBy = _currentUser.UserId?.ToString() ?? "System";

        _employeeRepository.Update(link);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Employee {UserId} detached from incoming letter {IncomingId} by {Caller}",
            userId, incomingId, _currentUser.UserId);
    }

    // ========== Scope + status helpers ==========

    /// <summary>
    /// Pin the caller's charity claim onto the filter — pin, never widen. A head-office
    /// caller without the claim keeps their explicit CharityId narrow (or sees all).
    /// </summary>
    private void ApplyCallerScope(IncomingFilterDto filter)
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
    private bool IsWithinCallerScope(Incoming incoming)
    {
        var callerCharity = _currentUser.CharityId;
        if (callerCharity.HasValue)
        {
            return incoming.FK_CharityId == callerCharity.Value;
        }

        var callerCountry = _currentUser.CountryId;
        if (callerCountry.HasValue)
        {
            return incoming.Charity != null && incoming.Charity.CountryId == callerCountry.Value;
        }

        return true;
    }

    /// <summary>
    /// FK existence checks (§21.S.2 / §21.U.4): a bogus department or assignee is a
    /// field-flagging 400 (ValidationException → the controller's errors map), never an
    /// FK-constraint 500. The assignee is an identity user (الموظف المناط به) — validated
    /// against the users store, not the employee register.
    /// </summary>
    private async Task EnsureReferencesExistAsync(int? departmentId, Guid? assignedUserId)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();

        if (departmentId.HasValue && await _departmentRepository.GetByIdAsync(departmentId.Value) == null)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                nameof(CreateIncomingDto.DepartmentId), "Routing department does not exist"));
        }

        if (assignedUserId.HasValue && await _userAppService.GetUserDetailAsync(assignedUserId.Value) == null)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                nameof(CreateIncomingDto.AssignedUserId), "Assigned employee does not exist"));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }

    /// <summary>The stored status must be one of the spec's tri-state terms (empty defaults at create).</summary>
    private static void EnsureKnownStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return;
        }

        if (Statuses.All(s => s.Id != status.Trim()))
        {
            throw new InvalidOperationException("Operation Faild: unknown letter status");
        }
    }
}
