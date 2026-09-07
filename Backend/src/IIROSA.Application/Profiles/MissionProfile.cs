using AutoMapper;
using IIROSA.Application.DTOs.MissionManagement;
using IIROSA.Domain.Entities;
using IIROSA.Domain.Entities.Lookups;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for Mission entity
/// Maps between Mission entity and DTOs for all use cases UC-8.1 through UC-8.13
/// </summary>
public class MissionProfile : Profile
{
    public MissionProfile()
    {
        // ========== Entity to DTO Mappings ==========

        /// <summary>
        /// Mission to MissionListDto (for grid display in UC-8.10)
        /// </summary>
        CreateMap<Mission, MissionListDto>()
            .ForMember(dest => dest.MissionType,
                opt => opt.MapFrom(src =>
                    src.MissionType != null ? src.MissionType.NameAr ?? src.MissionType.NameEn : null))
            .ForMember(dest => dest.MissionTimeType,
                opt => opt.MapFrom(src =>
                    src.MissionTimeType != null ? src.MissionTimeType.NameAr ?? src.MissionTimeType.NameEn : null))
            .ForMember(dest => dest.Region,
                opt => opt.MapFrom(src =>
                    src.Region != null ? src.Region.NameAr ?? src.Region.NameEn : null))
            .ForMember(dest => dest.Center,
                opt => opt.MapFrom(src =>
                    src.Center != null ? src.Center.NameAr ?? src.Center.NameEn : null))
            .ForMember(dest => dest.AssignedTo,
                opt => opt.MapFrom(src =>
                    src.AssignedUser != null ? src.AssignedUser.FullName : null))
            .ForMember(dest => dest.CountryName,
                opt => opt.MapFrom(src =>
                    src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null));

        /// <summary>
        /// Mission to MissionDetailDto (for detailed view, UC-MSN-07)
        /// </summary>
        CreateMap<Mission, MissionDetailDto>()
            .ForMember(dest => dest.MissionTypeName,
                opt => opt.MapFrom(src =>
                    src.MissionType != null ? src.MissionType.NameAr ?? src.MissionType.NameEn : null))
            .ForMember(dest => dest.MissionTimeTypeName,
                opt => opt.MapFrom(src =>
                    src.MissionTimeType != null ? src.MissionTimeType.NameAr ?? src.MissionTimeType.NameEn : null))
            .ForMember(dest => dest.MissionInterviewTypeName,
                opt => opt.MapFrom(src =>
                    src.MissionInterviewType != null ? src.MissionInterviewType.NameAr ?? src.MissionInterviewType.NameEn : null))
            .ForMember(dest => dest.CountryName,
                opt => opt.MapFrom(src =>
                    src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null))
            .ForMember(dest => dest.RegionName,
                opt => opt.MapFrom(src =>
                    src.Region != null ? src.Region.NameAr ?? src.Region.NameEn : null))
            .ForMember(dest => dest.CenterName,
                opt => opt.MapFrom(src =>
                    src.Center != null ? src.Center.NameAr ?? src.Center.NameEn : null))
            .ForMember(dest => dest.AssignedUserName,
                opt => opt.MapFrom(src =>
                    src.AssignedUser != null ? src.AssignedUser.FullName : null))
            .ForMember(dest => dest.AssignedUserEmail,
                opt => opt.MapFrom(src =>
                    src.AssignedUser != null ? src.AssignedUser.Email : null))
            .ForMember(dest => dest.CharityName,
                opt => opt.MapFrom(src =>
                    src.Charity != null ? src.Charity.Name : null))
            // Clean-named id keys — the entity columns are FK_*; without these the id
            // members would not map by name (detail screen's select values).
            .ForMember(dest => dest.MissionTypeId,
                opt => opt.MapFrom(src => src.FK_MissionTypeId))
            .ForMember(dest => dest.MissionTimeTypeId,
                opt => opt.MapFrom(src => src.FK_MissionTimeTypeId))
            .ForMember(dest => dest.MissionInterviewTypeId,
                opt => opt.MapFrom(src => src.FK_MissionInterviewTypeId))
            .ForMember(dest => dest.CountryId,
                opt => opt.MapFrom(src => src.FK_CountryId))
            .ForMember(dest => dest.RegionId,
                opt => opt.MapFrom(src => src.FK_RegionId))
            .ForMember(dest => dest.CenterId,
                opt => opt.MapFrom(src => src.FK_CenterId))
            .ForMember(dest => dest.AssignedToUserId,
                opt => opt.MapFrom(src => src.FK_UserId))
            .ForMember(dest => dest.CharityId,
                opt => opt.MapFrom(src => src.FK_CharityId));

        // ========== DTO to Entity Mappings ==========

        /// <summary>
        /// CreateMissionDto to Mission (UC-MSN-06). Wire keys are the clean names; the
        /// FK_* destinations are mapped explicitly. Charity ownership is stamped by the
        /// service, never taken from the payload.
        /// </summary>
        CreateMap<CreateMissionDto, Mission>()
            .ForMember(dest => dest.Id,
                opt => opt.Ignore()) // ID is generated by database
            .ForMember(dest => dest.CreatedOn,
                opt => opt.Ignore()) // Set by repository/base class
            .ForMember(dest => dest.CreatedBy,
                opt => opt.Ignore()) // Set by repository/base class
            .ForMember(dest => dest.UpdatedOn,
                opt => opt.Ignore()) // Set by repository/base class
            .ForMember(dest => dest.UpdatedBy,
                opt => opt.Ignore()) // Set by repository/base class
            .ForMember(dest => dest.IsMissionCompleted,
                opt => opt.MapFrom(src => false)) // Default value for new missions
            .ForMember(dest => dest.FK_MissionTypeId,
                opt => opt.MapFrom(src => src.MissionTypeId))
            .ForMember(dest => dest.FK_MissionTimeTypeId,
                opt => opt.MapFrom(src => src.MissionTimeTypeId))
            .ForMember(dest => dest.FK_MissionInterviewTypeId,
                opt => opt.MapFrom(src => src.MissionInterviewTypeId))
            .ForMember(dest => dest.FK_UserId,
                opt => opt.MapFrom(src => src.AssignedToUserId))
            .ForMember(dest => dest.FK_CountryId,
                opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.FK_RegionId,
                opt => opt.MapFrom(src => src.RegionId))
            .ForMember(dest => dest.FK_CenterId,
                opt => opt.MapFrom(src => src.CenterId))
            .ForMember(dest => dest.FK_CharityId,
                opt => opt.Ignore()) // Stamped server-side from the caller's claim
            .ForMember(dest => dest.MissionType,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.MissionTimeType,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.MissionInterviewType,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Country,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Region,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Center,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.AssignedUser,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Charity,
                opt => opt.Ignore()); // Navigation property

        /// <summary>
        /// UpdateMissionDto to Mission (UC-MSN-07) — the service applies fields manually
        /// (patch semantics); this map exists for symmetry and tooling.
        /// </summary>
        CreateMap<UpdateMissionDto, Mission>()
            .ForMember(dest => dest.Id,
                opt => opt.Ignore()) // ID should not be updated
            .ForMember(dest => dest.CreatedOn,
                opt => opt.Ignore()) // Should not be updated
            .ForMember(dest => dest.CreatedBy,
                opt => opt.Ignore()) // Should not be updated
            .ForMember(dest => dest.UpdatedOn,
                opt => opt.Ignore()) // Set by repository/base class
            .ForMember(dest => dest.UpdatedBy,
                opt => opt.Ignore()) // Set by repository/base class
            .ForMember(dest => dest.MissionType,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.MissionTimeType,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.MissionInterviewType,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Country,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Region,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Center,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.AssignedUser,
                opt => opt.Ignore()) // Navigation property
            .ForMember(dest => dest.Charity,
                opt => opt.Ignore()); // Navigation property

        // ========== Additional Mappings for Specific DTOs ==========

        /// <summary>
        /// MissionLocationDto to Mission (for location updates)
        /// </summary>
        CreateMap<MissionLocationDto, Mission>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.MissionTarget, opt => opt.Ignore())
            .ForMember(dest => dest.MissionDetails, opt => opt.Ignore())
            .ForMember(dest => dest.MissionType, opt => opt.Ignore())
            .ForMember(dest => dest.MissionTimeType, opt => opt.Ignore())
            .ForMember(dest => dest.MissionDate, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedUser, opt => opt.Ignore());
    }
}
