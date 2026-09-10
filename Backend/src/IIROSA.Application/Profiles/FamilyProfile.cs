using AutoMapper;
using IIROSA.Application.DTOs.Family;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for the families module members.
///
/// FamilyService has always called _mapper.Map&lt;FatherDto&gt;/&lt;ProviderDto&gt;/... but the maps
/// were never declared — member flows from the copied implementation threw at runtime. Declared
/// here (epic 7, UC-REF-03): the refugee register persists members through these same maps.
/// Resolved names follow the platform convention NameAr ?? NameEn.
/// </summary>
public class FamilyProfile : Profile
{
    public FamilyProfile()
    {
        // Father
        CreateMap<Father, FatherDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel != null ? src.EducationLevel.NameAr ?? src.EducationLevel.NameEn : null))
            .ForMember(dest => dest.HealthStatusName, opt => opt.MapFrom(src => src.HealthStatus != null ? src.HealthStatus.NameAr ?? src.HealthStatus.NameEn : null))
            .ForMember(dest => dest.NationalityName, opt => opt.MapFrom(src => src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null))
            .ForMember(dest => dest.DeathReasonName, opt => opt.MapFrom(src => src.DeathReason != null ? src.DeathReason.NameAr ?? src.DeathReason.NameEn : null));

        CreateMap<CreateFatherDto, Father>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FamilyId, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.EducationLevel, opt => opt.Ignore())
            .ForMember(dest => dest.HealthStatus, opt => opt.Ignore())
            .ForMember(dest => dest.DeathReason, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Mother
        CreateMap<Mother, MotherDto>()
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel != null ? src.EducationLevel.NameAr ?? src.EducationLevel.NameEn : null))
            .ForMember(dest => dest.HealthStatusName, opt => opt.MapFrom(src => src.HealthStatus != null ? src.HealthStatus.NameAr ?? src.HealthStatus.NameEn : null))
            .ForMember(dest => dest.NationalityName, opt => opt.MapFrom(src => src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null))
            .ForMember(dest => dest.DeathReasonName, opt => opt.MapFrom(src => src.DeathReason != null ? src.DeathReason.NameAr ?? src.DeathReason.NameEn : null));

        CreateMap<CreateMotherDto, Mother>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FamilyId, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.EducationLevel, opt => opt.Ignore())
            .ForMember(dest => dest.HealthStatus, opt => opt.Ignore())
            .ForMember(dest => dest.DeathReason, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Provider (incl. the refugee register extensions, epic 7 §12.S.2 اضافة معيل)
        CreateMap<Provider, ProviderDto>()
            .ForMember(dest => dest.ReasonOfRelationName, opt => opt.MapFrom(src => src.ReasonOfRelation != null ? src.ReasonOfRelation.NameAr ?? src.ReasonOfRelation.NameEn : null));

        CreateMap<CreateProviderDto, Provider>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FamilyId, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.ReasonOfRelation, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Orphan (SocialStatus — refugee register extension, epic 7 §12.S.2 اضافة ابن)
        CreateMap<Orphan, OrphanDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.HasValue ? (int)((DateTime.UtcNow - src.DateOfBirth.Value).TotalDays / 365.25) : (int?)null))
            .ForMember(dest => dest.FamilyCode, opt => opt.MapFrom(src => src.Family != null ? src.Family.Code : null))
            .ForMember(dest => dest.SponsorName, opt => opt.MapFrom(src => src.Sponsor != null ? src.Sponsor.FullName : null))
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel != null ? src.EducationLevel.NameAr ?? src.EducationLevel.NameEn : null))
            .ForMember(dest => dest.HealthStatusName, opt => opt.MapFrom(src => src.HealthStatus != null ? src.HealthStatus.NameAr ?? src.HealthStatus.NameEn : null))
            .ForMember(dest => dest.SocialStatusName, opt => opt.MapFrom(src => src.SocialStatus != null ? src.SocialStatus.NameAr ?? src.SocialStatus.NameEn : null));

        CreateMap<Orphan, OrphanListDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.HasValue ? (int)((DateTime.UtcNow - src.DateOfBirth.Value).TotalDays / 365.25) : (int?)null))
            .ForMember(dest => dest.FamilyCode, opt => opt.MapFrom(src => src.Family != null ? src.Family.Code : null))
            .ForMember(dest => dest.CharityName, opt => opt.Ignore());

        // UC-HOU-04 (§11.S.2): the hand-mapped FamilyDto lifted onto the housing detail
        // aggregate — Children is filled separately by the service (full OrphanDto rows).
        CreateMap<FamilyDto, HousingFamilyDetailDto>();

        CreateMap<CreateOrphanDto, Orphan>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Code, opt => opt.Ignore())
            .ForMember(dest => dest.FamilyId, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.FK_CharityId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Relative
        CreateMap<Relative, RelativeDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth != default ? (int)((DateTime.UtcNow - src.DateOfBirth).TotalDays / 365.25) : (int?)null))
            .ForMember(dest => dest.EducationLevelName, opt => opt.MapFrom(src => src.EducationLevel != null ? src.EducationLevel.NameAr ?? src.EducationLevel.NameEn : null))
            .ForMember(dest => dest.HealthStatusName, opt => opt.MapFrom(src => src.HealthStatus != null ? src.HealthStatus.NameAr ?? src.HealthStatus.NameEn : null));

        CreateMap<Relative, RelativeListDto>()
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth != default ? (int)((DateTime.UtcNow - src.DateOfBirth).TotalDays / 365.25) : (int?)null))
            .ForMember(dest => dest.HealthStatusName, opt => opt.MapFrom(src => src.HealthStatus != null ? src.HealthStatus.NameAr ?? src.HealthStatus.NameEn : null));

        CreateMap<CreateRelativeDto, Relative>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FamilyId, opt => opt.Ignore())
            .ForMember(dest => dest.Family, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
    }
}
