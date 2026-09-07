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

        // EducationLevel mappings (UC-ORP-11 orphan reference data)
        CreateMap<EducationLevel, EducationLevelDto>();

        CreateMap<CreateEducationLevelDto, EducationLevel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateEducationLevelDto, EducationLevel>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // HealthStatus mappings (UC-ORP-11 orphan reference data)
        CreateMap<HealthStatus, HealthStatusDto>();

        CreateMap<CreateHealthStatusDto, HealthStatus>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateHealthStatusDto, HealthStatus>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // RefuseReason mappings (epic 9, UC-ORR-08 periodic report refusal catalogue)
        CreateMap<RefuseReason, RefuseReasonDto>();

        CreateMap<CreateRefuseReasonDto, RefuseReason>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateRefuseReasonDto, RefuseReason>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Refugee register lookups (epic 7, UC-REF-03)
        CreateMap<HouseOwnership, HouseOwnershipDto>();
        CreateMap<CreateHouseOwnershipDto, HouseOwnership>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateHouseOwnershipDto, HouseOwnership>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<HouseStatus, HouseStatusDto>();
        CreateMap<CreateHouseStatusDto, HouseStatus>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateHouseStatusDto, HouseStatus>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<IncomeType, IncomeTypeDto>();
        CreateMap<CreateIncomeTypeDto, IncomeType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateIncomeTypeDto, IncomeType>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<SocialStatus, SocialStatusDto>();
        CreateMap<CreateSocialStatusDto, SocialStatus>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateSocialStatusDto, SocialStatus>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<Relation, RelationDto>();
        CreateMap<CreateRelationDto, Relation>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateRelationDto, Relation>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<ReasonOfRel, ReasonOfRelDto>();
        CreateMap<CreateReasonOfRelDto, ReasonOfRel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateReasonOfRelDto, ReasonOfRel>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // نوع السكن — shared catalogue (§12.S.2 refugee form)
        CreateMap<HousingType, HousingTypeDto>();
        CreateMap<CreateHousingTypeDto, HousingType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
        CreateMap<UpdateHousingTypeDto, HousingType>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // MaritalStatus mappings (UC-SYS-05 guardian reference data)
        CreateMap<MaritalStatus, MaritalStatusDto>();

        CreateMap<CreateMaritalStatusDto, MaritalStatus>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateMaritalStatusDto, MaritalStatus>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Job mappings (UC-SYS-09 guardian reference data)
        CreateMap<Job, JobDto>();

        CreateMap<CreateJobDto, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateJobDto, Job>()
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

        // MissionInterviewType mappings (UC-MSN-04)
        CreateMap<MissionInterviewType, MissionInterviewTypeDto>()
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateMissionInterviewTypeDto, MissionInterviewType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateMissionInterviewTypeDto, MissionInterviewType>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // MissionTimeType mappings (UC-MSN-05)
        CreateMap<MissionTimeType, MissionTimeTypeDto>()
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateMissionTimeTypeDto, MissionTimeType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateMissionTimeTypeDto, MissionTimeType>()
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

        // HousingBuilding mappings (UC-HOU-05)
        CreateMap<HousingBuilding, LookupDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
            .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Location));

        CreateMap<CreateLookupDto, HousingBuilding>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateLookupDto, HousingBuilding>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // HousingFlat mappings (UC-HOU-05)
        CreateMap<HousingFlat, LookupDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
            .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        CreateMap<CreateLookupDto, HousingFlat>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateLookupDto, HousingFlat>()
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
    }
}