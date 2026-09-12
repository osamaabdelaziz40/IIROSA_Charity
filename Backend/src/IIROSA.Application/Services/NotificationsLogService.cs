using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using IIROSA.Application.DTOs.Notifications;
using IIROSA.Application.Interfaces;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Interfaces;
using Framework.Core.SharedServices;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIROSA.Application.Services;

/// <summary>
/// NotificationsLog service (UC-NTF notifications epic).
///
/// Admin/SuperAdmin compose a web notification (title, message, audience = specific
/// users and/or whole charities); the service validates, persists one log row and
/// resolves the push audience — direct recipients plus every active user of each
/// selected charity. The API layer performs the live SignalR delivery using the
/// returned DeliveredToUserIds; only the UnitOfWork saves. Recipient matching for
/// the my-notifications read is a delimited CSV LIKE, translated by EF to SQL.
/// </summary>
public class NotificationsLogService : INotificationsLogService
{
    /// <summary>The seeded common.NotificationType row this feature stamps on every log.</summary>
    private const string WebNotificationTypeName = "Web";

    /// <summary>Upper bound for the page size a client can request in one call.</summary>
    private const int MaxPageSize = 200;

    private readonly INotificationsLogRepository _notificationsLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<NotificationsLogService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICommonsDbContext _commonsContext;
    private readonly IValidator<CreateNotificationsLogDto> _createValidator;
    private readonly IValidator<UpdateNotificationsLogDto> _updateValidator;

    public NotificationsLogService(
        INotificationsLogRepository notificationsLogRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<NotificationsLogService> logger,
        UserManager<ApplicationUser> userManager,
        ICommonsDbContext commonsContext,
        IValidator<CreateNotificationsLogDto> createValidator,
        IValidator<UpdateNotificationsLogDto> updateValidator)
    {
        _notificationsLogRepository = notificationsLogRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _userManager = userManager;
        _commonsContext = commonsContext;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ========== Reads ==========

    /// <summary>Admin register read — every pushed notification, newest first.</summary>
    public async Task<NotificationsLogPagedResult> GetFilteredAsync(NotificationsLogFilterDto filter)
    {
        filter ??= new NotificationsLogFilterDto();
        ClampPaging(filter);

        var (items, totalCount) = await _notificationsLogRepository.GetPagedAsync(
            BuildSearchExpression(filter.Search),
            q => q.OrderByDescending(n => n.CreatedOn),
            filter.Page,
            filter.PageSize);

        return new NotificationsLogPagedResult
        {
            Items = _mapper.Map<List<NotificationsLogListDto>>(items),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    /// <summary>
    /// Register statistics for the band above the admin notifications grid (UC-NTF list).
    /// </summary>
    /// <remarks>
    /// Deliberately not filter-reactive: the band describes the whole register, not the
    /// current search. Unscoped by design — GetFilteredAsync (the read this band sits above)
    /// is the admin register with no caller narrowing. One grouped round-trip over the
    /// no-tracking queryable; soft-deleted rows stay out via !IsDeleted, as in every read.
    /// </remarks>
    public async Task<NotificationsLogStatisticsDto> GetStatisticsAsync()
    {
        var monthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // One grouped round-trip for every scalar card; null when the register has no live rows.
        var totals = await _notificationsLogRepository.TableNoTracking
            .Where(n => !n.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                ToUsers = g.Count(n => n.IsUser),
                ToCharities = g.Count(n => n.IsCharity),
                AddedThisMonth = g.Count(n => n.CreatedOn >= monthStart)
            })
            .FirstOrDefaultAsync();

        return new NotificationsLogStatisticsDto
        {
            Total = totals?.Total ?? 0,
            ToUsers = totals?.ToUsers ?? 0,
            ToCharities = totals?.ToCharities ?? 0,
            AddedThisMonth = totals?.AddedThisMonth ?? 0
        };
    }

    /// <summary>Detail read for the edit screen.</summary>
    public async Task<NotificationsLogListDto?> GetByIdAsync(Guid id)
    {
        var notification = await _notificationsLogRepository.TableNoTracking
            .FirstOrDefaultAsync(n => n.Id == id);

        return notification == null ? null : _mapper.Map<NotificationsLogListDto>(notification);
    }

    /// <summary>
    /// The recipient's read: rows whose audience contains the user directly
    /// (IsUser + CSV of user ids) or through their charity (IsCharity + CSV of
    /// charity ids). The delimited token keeps ",<guid>," from matching a
    /// substring of a neighbouring id.
    /// </summary>
    public async Task<NotificationsLogPagedResult> GetForUserAsync(Guid userId, Guid? charityId, NotificationsLogFilterDto filter)
    {
        filter ??= new NotificationsLogFilterDto();
        ClampPaging(filter);

        var userToken = $",{userId},";
        var charityToken = charityId.HasValue ? $",{charityId.Value}," : string.Empty;
        var hasCharityScope = charityId.HasValue;

        // One lambda, no predicate composition: the search term and the charity
        // scope are captured constants EF evaluates client-side, so the SQL stays
        // a single WHERE with two LIKE matches on the delimited CSV columns.
        Expression<Func<NotificationsLog, bool>> combined = n =>
            (string.IsNullOrWhiteSpace(filter.Search)
                || n.Title.Contains(filter.Search!.Trim()) || n.Message.Contains(filter.Search!.Trim()))
            && ((n.IsUser && ("," + n.RecipientUserIds + ",").Contains(userToken))
                || (n.IsCharity && hasCharityScope && ("," + n.RecipientCharityIds + ",").Contains(charityToken)));

        var (items, totalCount) = await _notificationsLogRepository.GetPagedAsync(
            combined,
            q => q.OrderByDescending(n => n.CreatedOn),
            filter.Page,
            filter.PageSize);

        return new NotificationsLogPagedResult
        {
            Items = _mapper.Map<List<NotificationsLogListDto>>(items),
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }

    // ========== Writes ==========

    /// <summary>
    /// Create and stage the first push: validate, persist with the Web type id,
    /// resolve the audience for the hub.
    /// </summary>
    public async Task<NotificationsLogListDto> CreateAsync(CreateNotificationsLogDto dto)
    {
        await _createValidator.ValidateAndThrowAsync(dto);

        var notification = _mapper.Map<NotificationsLog>(dto);
        notification.NotificationTypeId = await ResolveWebNotificationTypeIdAsync();
        notification.SentCount = 1;
        notification.LastSentOn = DateTime.UtcNow;

        await _notificationsLogRepository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created notification {NotificationId} '{Title}' for {RecipientCount} recipients",
            notification.Id, notification.Title, dto.RecipientUserIds.Count + dto.RecipientCharityIds.Count);

        var result = _mapper.Map<NotificationsLogListDto>(notification);
        result.DeliveredToUserIds = await ResolveAudienceAsync(ParseIds(notification.RecipientUserIds), notification.RecipientCharityIds);
        return result;
    }

    /// <summary>Update a stored notification's content/audience — no push until resent.</summary>
    public async Task<NotificationsLogListDto> UpdateAsync(UpdateNotificationsLogDto dto)
    {
        await _updateValidator.ValidateAndThrowAsync(dto);

        var notification = await _notificationsLogRepository.Table
            .FirstOrDefaultAsync(n => n.Id == dto.Id);

        if (notification == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found for update", dto.Id);
            throw new InvalidOperationException($"Notification '{dto.Id}' was not found");
        }

        _mapper.Map(dto, notification);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated notification {NotificationId}", notification.Id);

        return _mapper.Map<NotificationsLogListDto>(notification);
    }

    /// <summary>
    /// Resend: re-resolve the audience from the stored selection and bump the
    /// delivery bookkeeping. The content is resent as stored — editing and
    /// resending are separate acts.
    /// </summary>
    public async Task<NotificationsLogListDto> ResendAsync(Guid id)
    {
        var notification = await _notificationsLogRepository.Table
            .FirstOrDefaultAsync(n => n.Id == id);

        if (notification == null)
        {
            _logger.LogWarning("Notification {NotificationId} not found for resend", id);
            throw new InvalidOperationException($"Notification '{id}' was not found");
        }

        notification.SentCount++;
        notification.LastSentOn = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Resending notification {NotificationId} (send #{SentCount})",
            notification.Id, notification.SentCount);

        var result = _mapper.Map<NotificationsLogListDto>(notification);
        result.DeliveredToUserIds = await ResolveAudienceAsync(ParseIds(notification.RecipientUserIds), notification.RecipientCharityIds);
        return result;
    }

    // ========== Helpers ==========

    /// <summary>Paging bounds arrive from the wire — clamp before Skip/Take.</summary>
    private static void ClampPaging(NotificationsLogFilterDto filter)
    {
        filter.Page = Math.Max(1, filter.Page);
        filter.PageSize = Math.Clamp(filter.PageSize, 1, MaxPageSize);
    }

    private static Expression<Func<NotificationsLog, bool>>? BuildSearchExpression(string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return null;
        }

        var term = search.Trim();
        return n => n.Title.Contains(term) || n.Message.Contains(term);
    }

    /// <summary>CSV of Guids → list, dropping empties and unparsable tokens.</summary>
    private static List<Guid> ParseIds(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
        {
            return new List<Guid>();
        }

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => Guid.TryParse(t, out _))
            .Select(Guid.Parse)
            .ToList();
    }

    /// <summary>
    /// Direct recipients plus every active user of each selected charity,
    /// de-duplicated — the ids the hub notification is addressed to.
    /// </summary>
    private async Task<List<string>> ResolveAudienceAsync(List<Guid> directUserIds, string recipientCharityIdsCsv)
    {
        var audience = new HashSet<string>(directUserIds.Select(id => id.ToString()));

        var charityIds = recipientCharityIdsCsv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(t => Guid.TryParse(t, out _))
            .Select(Guid.Parse)
            .Distinct()
            .ToList();

        if (charityIds.Count > 0)
        {
            var charityUserIds = await _userManager.Users
                .Where(u => u.CharityId != null && charityIds.Contains(u.CharityId.Value) && u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            foreach (var userId in charityUserIds)
            {
                audience.Add(userId.ToString());
            }
        }

        return audience.ToList();
    }

    /// <summary>The common.NotificationType "Web" row this feature stamps on every log.</summary>
    private async Task<int> ResolveWebNotificationTypeIdAsync()
    {
        var webType = await _commonsContext.Set<Framework.Core.SharedServices.Entities.NotificationType>()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.NameEn == WebNotificationTypeName);

        return webType?.Id ?? 0;
    }
}
