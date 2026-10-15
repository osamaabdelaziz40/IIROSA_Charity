using AutoMapper;
using IIROSA.Application.DTOs.OrphanReportCycles;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// OrphanReportCycle mappings (UC-ORC). The read DTO is assembled by the service
/// (report counts and progress are computed, not mapped), so the profile carries the
/// create mapping; the entity→DTO leg stays explicit in DecorateWithCountsAsync to keep
/// the computed columns from silently drifting from their queries.
/// </summary>
public class OrphanReportCycleProfile : Profile
{
    public OrphanReportCycleProfile()
    {
        CreateMap<CreateOrphanReportCycleDto, OrphanReportCycle>()
            .ForMember(d => d.TitleAr, o => o.MapFrom(s => s.TitleAr.Trim()))
            .ForMember(d => d.TitleEn, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.TitleEn) ? null : s.TitleEn.Trim()))
            .ForMember(d => d.DescriptionAr, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.DescriptionAr) ? null : s.DescriptionAr.Trim()))
            .ForMember(d => d.DescriptionEn, o => o.MapFrom(s => string.IsNullOrWhiteSpace(s.DescriptionEn) ? null : s.DescriptionEn.Trim()));
    }
}
