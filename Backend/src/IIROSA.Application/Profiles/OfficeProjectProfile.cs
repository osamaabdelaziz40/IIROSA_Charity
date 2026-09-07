using AutoMapper;
using IIROSA.Application.DTOs.OfficeProjectManagement;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for the OfficeProject entity (UC-OFP-01…06).
///
/// The entity keeps its legacy `FK_`-prefixed columns (database unchanged); the DTOs use clean
/// names so the camelCase wire contract reads `countryId`, not `fK_CountryId`. Every FK pair is
/// bridged with an explicit ForMember below — without them the name-based default would silently
/// skip the columns and leave the FKs null.
/// </summary>
public class OfficeProjectProfile : Profile
{
    public OfficeProjectProfile()
    {
        // ========== Entity → DTO ==========

        // Grid row (UC-OFP-01)
        CreateMap<OfficeProject, OfficeProjectListDto>()
            .ForMember(dest => dest.ProjectType,
                opt => opt.MapFrom(src =>
                    src.OfficeProjectType != null ? src.OfficeProjectType.NameAr ?? src.OfficeProjectType.NameEn : null))
            .ForMember(dest => dest.Region,
                opt => opt.MapFrom(src =>
                    src.Region != null ? src.Region.NameAr ?? src.Region.NameEn : null))
            .ForMember(dest => dest.Center,
                opt => opt.MapFrom(src =>
                    src.Center != null ? src.Center.NameAr ?? src.Center.NameEn : null))
            .ForMember(dest => dest.Village,
                opt => opt.MapFrom(src => src.VillageName))
            .ForMember(dest => dest.AssignedCharity,
                opt => opt.MapFrom(src =>
                    src.Charity != null ? src.Charity.Name : null))
            .ForMember(dest => dest.CountryName,
                opt => opt.MapFrom(src =>
                    src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null));

        // Detail (UC-OFP-04) — navigation names come from the includes; the FK ids feed the edit
        // form's patch step, so both must survive the rename.
        CreateMap<OfficeProject, OfficeProjectDetailDto>()
            .ForMember(dest => dest.OfficeProjectTypeId, opt => opt.MapFrom(src => src.FK_OfficeProjectTypeId))
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.FK_CountryId))
            .ForMember(dest => dest.RegionId, opt => opt.MapFrom(src => src.FK_RegionId))
            .ForMember(dest => dest.CenterId, opt => opt.MapFrom(src => src.FK_CenterId))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.FK_CharityId))
            .ForMember(dest => dest.AttachedFileId, opt => opt.MapFrom(src => src.FK_AttachedFileId))
            .ForMember(dest => dest.ProjectReportFileId, opt => opt.MapFrom(src => src.FK_ProjectReportFileId))
            .ForMember(dest => dest.OfficeProjectTypeName,
                opt => opt.MapFrom(src =>
                    src.OfficeProjectType != null ? src.OfficeProjectType.NameAr ?? src.OfficeProjectType.NameEn : null))
            .ForMember(dest => dest.CountryName,
                opt => opt.MapFrom(src =>
                    src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null))
            .ForMember(dest => dest.RegionName,
                opt => opt.MapFrom(src =>
                    src.Region != null ? src.Region.NameAr ?? src.Region.NameEn : null))
            .ForMember(dest => dest.CenterName,
                opt => opt.MapFrom(src =>
                    src.Center != null ? src.Center.NameAr ?? src.Center.NameEn : null))
            .ForMember(dest => dest.CharityName,
                opt => opt.MapFrom(src =>
                    src.Charity != null ? src.Charity.Name : null));

        // ========== DTO → Entity ==========

        // Create (UC-OFP-03) — audit fields and navigation properties belong to the base/pipeline;
        // the two attachment FKs are resolved by the service from the uploaded attachments.
        CreateMap<CreateOfficeProjectDto, OfficeProject>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_OfficeProjectTypeId, opt => opt.MapFrom(src => src.OfficeProjectTypeId))
            .ForMember(dest => dest.FK_CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.FK_RegionId, opt => opt.MapFrom(src => src.RegionId))
            .ForMember(dest => dest.FK_CenterId, opt => opt.MapFrom(src => src.CenterId))
            .ForMember(dest => dest.FK_CharityId, opt => opt.MapFrom(src => src.CharityId))
            .ForMember(dest => dest.FK_AttachedFileId, opt => opt.Ignore())
            .ForMember(dest => dest.FK_ProjectReportFileId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.OfficeProjectType, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Region, opt => opt.Ignore())
            .ForMember(dest => dest.Center, opt => opt.Ignore())
            .ForMember(dest => dest.Charity, opt => opt.Ignore());

        // Update (UC-OFP-04) has no DTO→entity map on purpose: the service patches non-null
        // values onto the loaded entity so an omitted field never clobbers stored data.
    }
}
