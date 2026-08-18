using AutoMapper;
using IIROSA.Application.DTOs.CheckManagement;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for Check entity
/// Maps between Check entity and DTOs
/// </summary>
public class CheckProfile : Profile
{
    public CheckProfile()
    {
        // Entity to DTO mappings
        CreateMap<Check, CheckListDto>()
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null));

        CreateMap<Check, CheckDetailDto>()
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null))
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.ApprovedBy != null ? "Approved User" : null)) // TODO: Get from user service
            .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.CreatedBy != null ? "Creator" : null)) // TODO: Get from user service
            .ForMember(dest => dest.ModifierName, opt => opt.MapFrom(src => src.UpdatedBy != null ? "Modifier" : null)); // TODO: Get from user service

        // DTO to Entity mappings
        CreateMap<CreateCheckDto, Check>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CheckStatus, opt => opt.MapFrom(src => "Pending"))
            .ForMember(dest => dest.CanModify, opt => opt.Ignore())
            .ForMember(dest => dest.CanBeCleared, opt => opt.Ignore())
            .ForMember(dest => dest.CanBeVoided, opt => opt.Ignore());

        CreateMap<UpdateCheckDto, Check>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CheckStatus, opt => opt.Ignore())
            .ForMember(dest => dest.CanModify, opt => opt.Ignore())
            .ForMember(dest => dest.CanBeCleared, opt => opt.Ignore())
            .ForMember(dest => dest.CanBeVoided, opt => opt.Ignore())
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }
}
