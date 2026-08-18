using AutoMapper;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.Base;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for lookup entities
/// Maps between lookup entities and DTOs
/// </summary>
public class LookupProfile : Profile
{
    public LookupProfile()
    {
        // Country mappings
        CreateMap<Country, CountryDto>()
            .ForMember(dest => dest.RegionCount, opt => opt.MapFrom(src => src.Regions != null ? src.Regions.Count : 0))
            .ForMember(dest => dest.CenterCount, opt => opt.MapFrom(src => src.Centers != null ? src.Centers.Count : 0))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateCountryDto, Country>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Regions, opt => opt.Ignore())
            .ForMember(dest => dest.Centers, opt => opt.Ignore())
            .ForMember(dest => dest.Cities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateCountryDto, Country>()
            .ForMember(dest => dest.Regions, opt => opt.Ignore())
            .ForMember(dest => dest.Centers, opt => opt.Ignore())
            .ForMember(dest => dest.Cities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Region mappings
        CreateMap<Region, RegionDto>()
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.NameAr : null))
            .ForMember(dest => dest.CenterCount, opt => opt.MapFrom(src => src.Centers != null ? src.Centers.Count : 0))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateRegionDto, Region>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Centers, opt => opt.Ignore())
            .ForMember(dest => dest.Charities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateRegionDto, Region>()
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Centers, opt => opt.Ignore())
            .ForMember(dest => dest.Charities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Center mappings
        CreateMap<Center, CenterDto>()
            .ForMember(dest => dest.RegionName, opt => opt.MapFrom(src => src.Region != null ? src.Region.NameAr : null))
            .ForMember(dest => dest.CountryName, opt => opt.MapFrom(src => src.Country != null ? src.Country.NameAr : null))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateCenterDto, Center>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CountryId, opt => opt.Ignore())
            .ForMember(dest => dest.Region, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Charities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateCenterDto, Center>()
            .ForMember(dest => dest.CountryId, opt => opt.Ignore())
            .ForMember(dest => dest.Region, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Charities, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Department mappings
        CreateMap<Department, DepartmentDto>();

        CreateMap<CreateDepartmentDto, Department>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateDepartmentDto, Department>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // MissionType mappings
        CreateMap<MissionType, MissionTypeDto>()
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.TypeDescription))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateMissionTypeDto, MissionType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateMissionTypeDto, MissionType>()
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // ProjectType mappings
        CreateMap<ProjectType, ProjectTypeDto>()
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.TypeDescription))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateProjectTypeDto, ProjectType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateProjectTypeDto, ProjectType>()
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Bank mappings
        CreateMap<Bank, BankDto>()
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateBankDto, Bank>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateBankDto, Bank>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // NGOType mappings
        CreateMap<NGOType, NGOTypeDto>()
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.TypeDescription))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateNGOTypeDto, NGOType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateNGOTypeDto, NGOType>()
            .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // OfficeProjectType mappings
        CreateMap<OfficeProjectType, LookupDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
            .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.TypeDescription));

        CreateMap<CreateLookupDto, OfficeProjectType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateLookupDto, OfficeProjectType>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
    }
}