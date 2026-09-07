using AutoMapper;
using IIROSA.Application.DTOs.Family;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for the GuardianChangeRequest aggregate (UC-FAM-09/10)
/// </summary>
public class GuardianChangeRequestProfile : Profile
{
    public GuardianChangeRequestProfile()
    {
        // The review-queue row: the raising charity's Arabic name rides along for الجمعيه;
        // the family code is a display-only convenience for the reviewer.
        CreateMap<GuardianChangeRequest, GuardianChangeRequestListDto>()
            .ForMember(dest => dest.CharityName,
                opt => opt.MapFrom(src => src.Charity != null ? (src.Charity.Name) : null))
            .ForMember(dest => dest.FamilyCode,
                opt => opt.MapFrom(src => src.Family != null ? src.Family.Code : null));
    }
}
