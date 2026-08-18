using AutoMapper;
using IIROSA.Domain.Entities;
using IIROSA.Application.DTOs.IncomingOutgoing;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for Incoming and Outgoing correspondence entities
/// </summary>
public class IncomingOutgoingMappingProfile : Profile
{
    public IncomingOutgoingMappingProfile()
    {
        // Incoming Letter Mappings
        CreateMap<Incoming, IncomingDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.UserName, opt => opt.Ignore()) // Map from user service
            .ForMember(dest => dest.UploadedFileName, opt => opt.MapFrom(src => src.UploadedFile != null ? src.UploadedFile.FileName : null));

        CreateMap<Incoming, IncomingListDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null));

        // DTO to Entity mappings for Incoming
        CreateMap<CreateIncomingDto, Incoming>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedFile, opt => opt.Ignore())
            .ForMember(dest => dest.OutgoingLetter, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore());

        CreateMap<UpdateIncomingDto, Incoming>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedFile, opt => opt.Ignore())
            .ForMember(dest => dest.OutgoingLetter, opt => opt.Ignore())
            .ForMember(dest => dest.Replies, opt => opt.Ignore());

        // Outgoing Letter Mappings
        CreateMap<Outgoing, OutgoingDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.UploadedFileName, opt => opt.MapFrom(src => src.UploadedFile != null ? src.UploadedFile.FileName : null))
            .ForMember(dest => dest.IncomingLetterSubject, opt => opt.MapFrom(src => src.IncomingLetter != null ? src.IncomingLetter.Subject : null));

        CreateMap<Outgoing, OutgoingListDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.HasReply, opt => opt.MapFrom(src => src.IncomingId != null));

        // DTO to Entity mappings for Outgoing
        CreateMap<CreateOutgoingDto, Outgoing>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedFile, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.IncomingLetter, opt => opt.Ignore())
            .ForMember(dest => dest.ChildOutGoings, opt => opt.Ignore());

        CreateMap<UpdateOutgoingDto, Outgoing>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.UploadedFile, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.IncomingLetter, opt => opt.Ignore())
            .ForMember(dest => dest.ChildOutGoings, opt => opt.Ignore());
    }
}
