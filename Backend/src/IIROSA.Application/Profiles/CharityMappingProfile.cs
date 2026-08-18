using AutoMapper;
using IIROSA.Domain.Entities;
using IIROSA.Application.DTOs.Charity;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for Charity entity
/// </summary>
public class CharityMappingProfile : Profile
{
    public CharityMappingProfile()
    {
        // Entity to DTO mappings
        CreateMap<Charity, CharityDto>()
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.NameAr : null))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region != null ? src.Region.NameAr : null))
            .ForMember(dest => dest.CenterName, opt => opt.MapFrom(src => src.Center != null ? src.Center.NameAr : null))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.NameAr : null))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City != null ? src.City.NameAr : null))
            .ForMember(dest => dest.Username, opt => opt.Ignore()) // Will be loaded manually in service
            .ForMember(dest => dest.FamilyCount, opt => opt.Ignore())
            .ForMember(dest => dest.OrphanCount, opt => opt.Ignore())
            .ForMember(dest => dest.SponsorCount, opt => opt.Ignore());

        CreateMap<Charity, CharityListDto>()
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region != null ? src.Region.NameAr : null))
            .ForMember(dest => dest.CenterName, opt => opt.MapFrom(src => src.Center != null ? src.Center.NameAr : null));

        CreateMap<Charity, CharityProfileDto>()
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? (src.Country.NameAr ?? src.Country.NameEn) : null))
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region != null ? (src.Region.NameAr ?? src.Region.NameEn) : null))
            .ForMember(dest => dest.CenterName, opt => opt.MapFrom(src => src.Center != null ? (src.Center.NameAr ?? src.Center.NameEn) : null))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? (src.Bank.NameAr ?? src.Bank.NameEn) : null))
            .ForMember(dest => dest.Username, opt => opt.Ignore()) // Will be loaded manually in service
            .ForMember(dest => dest.FamilyCount, opt => opt.Ignore())
            .ForMember(dest => dest.OrphanCount, opt => opt.Ignore())
            .ForMember(dest => dest.SponsorCount, opt => opt.Ignore());

        // DTO to Entity mappings (for create/update)
        CreateMap<CreateCharityDto, Charity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Families, opt => opt.Ignore())
            .ForMember(dest => dest.Orphans, opt => opt.Ignore())
            .ForMember(dest => dest.Sponsors, opt => opt.Ignore());

        CreateMap<UpdateCharityDto, Charity>()
            .ForMember(dest => dest.Families, opt => opt.Ignore())
            .ForMember(dest => dest.Orphans, opt => opt.Ignore())
            .ForMember(dest => dest.Sponsors, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.IsLocked, opt => opt.Ignore())
            .ForMember(dest => dest.IsAddEnabled, opt => opt.Ignore())
            .ForMember(dest => dest.IsUpdateEnabled, opt => opt.Ignore());
    }
}