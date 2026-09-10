using AutoMapper;
using IIROSA.Application.DTOs.Notifications;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// NotificationsLog mapping profile. The recipient CSV lists are a storage
/// detail: the wire always speaks List&lt;Guid&gt;, the entity always stores CSV.
/// </summary>
public class NotificationsLogProfile : Profile
{
    public NotificationsLogProfile()
    {
        CreateMap<NotificationsLog, NotificationsLogListDto>()
            .ForMember(d => d.RecipientUserIds,
                o => o.MapFrom(s => SplitIds(s.RecipientUserIds)))
            .ForMember(d => d.RecipientCharityIds,
                o => o.MapFrom(s => SplitIds(s.RecipientCharityIds)));

        CreateMap<CreateNotificationsLogDto, NotificationsLog>()
            .ForMember(d => d.RecipientUserIds,
                o => o.MapFrom(s => JoinIds(s.RecipientUserIds)))
            .ForMember(d => d.RecipientCharityIds,
                o => o.MapFrom(s => JoinIds(s.RecipientCharityIds)))
            // Delivery bookkeeping is service-owned, never bound from the wire.
            .ForMember(d => d.SentCount, o => o.Ignore())
            .ForMember(d => d.LastSentOn, o => o.Ignore())
            .ForMember(d => d.NotificationTypeId, o => o.Ignore());

        CreateMap<UpdateNotificationsLogDto, NotificationsLog>()
            .IncludeBase<CreateNotificationsLogDto, NotificationsLog>();
    }

    /// <summary>CSV of Guids → list, dropping empties and unparsable tokens.</summary>
    private static List<Guid> SplitIds(string? csv)
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

    /// <summary>List of Guids → CSV ("" for an empty list).</summary>
    private static string JoinIds(IEnumerable<Guid>? ids)
    {
        return ids == null ? string.Empty : string.Join(',', ids.Select(id => id.ToString()).Distinct());
    }
}
