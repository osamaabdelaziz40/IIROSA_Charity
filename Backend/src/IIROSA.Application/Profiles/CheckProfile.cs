using AutoMapper;
using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for the Check entity (chapter 16, UC-CHQ).
/// Wire keys are clean (bankId/charityId/…) — the FK_ prefix stays a persistence detail.
/// </summary>
public class CheckProfile : Profile
{
    public CheckProfile()
    {
        // Entity → DTO
        CreateMap<Check, CheckListDto>()
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.FK_CharityId))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Notes));

        CreateMap<Check, CheckDetailDto>()
            .ForMember(dest => dest.BankId, opt => opt.MapFrom(src => src.FK_BankId))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null))
            .ForMember(dest => dest.ChequeBeneficiaryId, opt => opt.MapFrom(src => src.FK_ChequeBeneficiaryId))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.FK_CharityId))
            .ForMember(dest => dest.CharityName, opt => opt.MapFrom(src => src.Charity != null ? src.Charity.Name : null))
            .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Notes));

        // DTO → Entity
        CreateMap<CreateCheckDto, Check>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_BankId, opt => opt.MapFrom(src => src.BankId))
            .ForMember(dest => dest.FK_ChequeBeneficiaryId, opt => opt.MapFrom(src => src.ChequeBeneficiaryId))
            .ForMember(dest => dest.FK_CharityId, opt => opt.MapFrom(src => src.CharityId))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Comment))
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Bank, opt => opt.Ignore())
            .ForMember(dest => dest.ChequeBeneficiary, opt => opt.Ignore())
            .ForMember(dest => dest.Charity, opt => opt.Ignore());

        // Update DTO inherits Create's shape; the id never crosses the map (loaded entity keeps it).
        CreateMap<UpdateCheckDto, Check>()
            .IncludeBase<CreateCheckDto, Check>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
