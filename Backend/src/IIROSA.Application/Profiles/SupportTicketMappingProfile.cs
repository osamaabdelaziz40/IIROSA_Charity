using AutoMapper;
using IIROSA.Application.DTOs.TechnicalSupport;
using IIROSA.Application.DTOs.LookupManagement;
using IIROSA.Domain.Entities.TechnicalSupport;
using IIROSA.Domain.Entities.TechnicalSupport.Lookups;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for Technical Support entities
/// </summary>
public class SupportTicketMappingProfile : Profile
{
    public SupportTicketMappingProfile()
    {
        // SupportTicket Mappings
        CreateMap<SupportTicket, SupportTicketDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? (src.Category.NameAr ?? src.Category.NameEn) : null))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority != null ? (src.Priority.NameAr ?? src.Priority.NameEn) : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? (src.Status.NameAr ?? src.Status.NameEn) : null))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserId))
            .ForMember(dest => dest.CreatedByEmail, opt => opt.MapFrom(src => src.CreatedByUserId))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo))
            .ForMember(dest => dest.ResponseCount, opt => opt.Ignore());

        CreateMap<SupportTicket, SupportTicketListDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? (src.Category.NameAr ?? src.Category.NameEn) : null))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority != null ? (src.Priority.NameAr ?? src.Priority.NameEn) : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? (src.Status.NameAr ?? src.Status.NameEn) : null))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserId))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo))
            .ForMember(dest => dest.ResponseCount, opt => opt.Ignore())
            .ForMember(dest => dest.PriorityColor, opt => opt.MapFrom(src => src.Priority.ColorCode))
            .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src => src.Status.ColorCode));

        CreateMap<SupportTicket, SupportTicketDetailDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? (src.Category.NameAr ?? src.Category.NameEn) : null))
            .ForMember(dest => dest.PriorityName, opt => opt.MapFrom(src => src.Priority != null ? (src.Priority.NameAr ?? src.Priority.NameEn) : null))
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? (src.Status.NameAr ?? src.Status.NameEn) : null))
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUserId))
            .ForMember(dest => dest.CreatedByEmail, opt => opt.MapFrom(src => src.CreatedByUserId))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo))
            .ForMember(dest => dest.Responses, opt => opt.MapFrom(src => src.Responses))
            .ForMember(dest => dest.PublicResponses, opt => opt.Ignore())
            .ForMember(dest => dest.InternalNotes, opt => opt.Ignore());

        // Create mappings
        CreateMap<CreateSupportTicketDto, SupportTicket>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsSolved, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => 1))
            .ForMember(dest => dest.ResolutionDescription, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedBy, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateSupportTicketDto, SupportTicket>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsSolved, opt => opt.Ignore())
            .ForMember(dest => dest.ResolutionDescription, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedOn, opt => opt.Ignore())
            .ForMember(dest => dest.ResolvedBy, opt => opt.Ignore())
            .ForMember(dest => dest.BrowserInfo, opt => opt.Ignore())
            .ForMember(dest => dest.PageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.UserAction, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedTo, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentFilePath, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentFileName, opt => opt.Ignore())
            .ForMember(dest => dest.AttachmentFileSize, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Responses, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // TicketResponse Mappings
        CreateMap<TicketResponse, TicketResponseDto>().ReverseMap();

        CreateMap<CreateTicketResponseDto, TicketResponse>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.RespondedByUserId, opt => opt.Ignore())
            .ForMember(dest => dest.ResponderName, opt => opt.Ignore())
            .ForMember(dest => dest.ResponderEmail, opt => opt.Ignore())
            .ForMember(dest => dest.Ticket, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        // Lookup Entity Mappings
        CreateMap<SupportTicketCategory, LookupDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NameAr ?? src.NameEn))
            .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
            .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<SupportTicketPriority, LookupDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NameAr ?? src.NameEn))
            .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
            .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<SupportTicketStatus, LookupDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NameAr ?? src.NameEn))
            .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
            .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}