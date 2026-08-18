using AutoMapper;
using IIROSA.Domain.Entities;
using IIROSA.Application.DTOs.OrphanPayment;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for OrphanPayment entity
/// </summary>
public class OrphanPaymentProfile : Profile
{
    public OrphanPaymentProfile()
    {
        // Entity to DTO mappings
        CreateMap<OrphanPayment, OrphanPaymentDto>()
            .ForMember(dest => dest.Orphans, opt => opt.Ignore()) // Loaded manually in service
            .ForMember(dest => dest.OrphanCount, opt => opt.Ignore()) // Loaded manually in service
            .ForMember(dest => dest.OrphanCountByCharity, opt => opt.Ignore()) // Loaded manually in service
            .ForMember(dest => dest.OrphanCountByRegion, opt => opt.Ignore()); // Loaded manually in service

        CreateMap<OrphanPayment, OrphanPaymentListDto>()
            .ForMember(dest => dest.OrphanCount, opt => opt.Ignore()) // Loaded manually in service
            .ForMember(dest => dest.CreatedByName, opt => opt.Ignore()); // Loaded manually if needed

        CreateMap<OrphanPaymentItem, OrphanPaymentItemDto>()
            .ForMember(dest => dest.OrphanCode, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.Code : null))
            .ForMember(dest => dest.OrphanFullName, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.FullName : null))
            .ForMember(dest => dest.OrphanFamilyName, opt => opt.MapFrom(src => src.Orphan != null && src.Orphan.Family != null ? src.Orphan.Family.HeadOfFamily : null))
            .ForMember(dest => dest.OrphanAge, opt => opt.Ignore()) // Calculated in service
            .ForMember(dest => dest.OrphanGender, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.Gender : null))
            .ForMember(dest => dest.OrphanEducationLevel, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.EducationLevel : null))
            .ForMember(dest => dest.OrphanMonthlyAmount, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.MonthlyAmount : null))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.FK_CharityId : null))
            .ForMember(dest => dest.CharityName, opt => opt.Ignore()) // Loaded from charity lookup if needed
            .ForMember(dest => dest.RegionId, opt => opt.Ignore()) // Loaded from family or orphan lookup
            .ForMember(dest => dest.RegionName, opt => opt.Ignore())
            .ForMember(dest => dest.CenterId, opt => opt.Ignore())
            .ForMember(dest => dest.CenterName, opt => opt.Ignore())
            .ForMember(dest => dest.SponsorId, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.SponsorId : null))
            .ForMember(dest => dest.SponsorName, opt => opt.Ignore()) // Loaded from sponsor lookup if needed
            .ForMember(dest => dest.SponsorshipStartDate, opt => opt.MapFrom(src => src.Orphan != null ? src.Orphan.SponsorshipStartDate : null))
            .ForMember(dest => dest.SponsorshipStatus, opt => opt.Ignore()); // Determined in service

        // DTO to Entity mappings (for create/update)
        CreateMap<CreateOrphanPaymentDto, OrphanPayment>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Orphans, opt => opt.Ignore())
            .ForMember(dest => dest.IsBatchUploaded, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.UploadDate, opt => opt.Ignore());

        CreateMap<UpdateOrphanPaymentDto, OrphanPayment>()
            .ForMember(dest => dest.Orphans, opt => opt.Ignore());

        // Orphan to OrphanForPaymentListDto mapping
        CreateMap<Orphan, OrphanForPaymentListDto>()
            .ForMember(dest => dest.IsInGroup, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.OrphanPaymentItemId, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.Age, opt => opt.Ignore()) // Calculated in service
            .ForMember(dest => dest.SponsorshipStatus, opt => opt.Ignore()) // Determined in service
            .ForMember(dest => dest.FamilyName, opt => opt.MapFrom(src => src.Family != null ? src.Family.HeadOfFamily : null))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.FK_CharityId))
            .ForMember(dest => dest.CharityName, opt => opt.Ignore()) // Loaded from charity lookup if needed
            .ForMember(dest => dest.RegionId, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.RegionName, opt => opt.Ignore())
            .ForMember(dest => dest.CenterId, opt => opt.Ignore()) // Set in service
            .ForMember(dest => dest.CenterName, opt => opt.Ignore());
    }
}
