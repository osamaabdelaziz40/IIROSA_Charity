using AutoMapper;
using IIROSA.Application.DTOs.IncomingOutgoing;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper profile for the correspondence entities (epic 16, UC-COR-01…19).
/// Entity → DTO only: the services build entities by hand so the serial and charity
/// stamping stay explicit, so there are no DTO → entity maps here.
/// </summary>
public class IncomingOutgoingMappingProfile : Profile
{
    public IncomingOutgoingMappingProfile()
    {
        // ========== Incoming (§21.S.1 / §21.S.2) ==========

        CreateMap<Incoming, IncomingDto>()
            .ForMember(dest => dest.SerialTxt, opt => opt.MapFrom(src => src.Serial_Txt))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.FK_DepartmentId))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.AssignedUserId, opt => opt.MapFrom(src => src.FK_UserId))
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : null))
            .ForMember(dest => dest.OutgoingLetterNumber, opt => opt.MapFrom(src =>
                src.OutgoingLetter != null ? src.OutgoingLetter.OutGoingNumber ?? src.OutgoingLetter.OutGoingId : null))
            .ForMember(dest => dest.UploadedFileName, opt => opt.MapFrom(src => src.UploadedFile != null ? src.UploadedFile.FileName : null))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.FK_CharityId))
            .ForMember(dest => dest.CharityName, opt => opt.MapFrom(src => src.Charity != null ? src.Charity.Name : null));

        CreateMap<Incoming, IncomingListDto>()
            .ForMember(dest => dest.SerialTxt, opt => opt.MapFrom(src => src.Serial_Txt))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.FK_DepartmentId))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.AssignedUserName, opt => opt.MapFrom(src => src.AssignedUser != null ? src.AssignedUser.FullName : null))
            .ForMember(dest => dest.UploadedFileId, opt => opt.MapFrom(src => src.UploadedFileId))
            .ForMember(dest => dest.UploadedFileName, opt => opt.MapFrom(src => src.UploadedFile != null ? src.UploadedFile.FileName : null));

        // ========== Outgoing (§21.S.4 / §21.S.5) ==========

        CreateMap<Outgoing, OutgoingDto>()
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.Fk_DepartmentId))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.UploadedFileName, opt => opt.MapFrom(src => src.UploadedFile != null ? src.UploadedFile.FileName : null))
            .ForMember(dest => dest.OutgoingCategoryId, opt => opt.MapFrom(src => src.OutgoingCategoryId))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.IncomingLetterNumber, opt => opt.MapFrom(src => src.IncomingLetter != null ? src.IncomingLetter.LetterNumber : null))
            .ForMember(dest => dest.IncomingLetterSubject, opt => opt.MapFrom(src => src.IncomingLetter != null ? src.IncomingLetter.Subject : null))
            .ForMember(dest => dest.CharityId, opt => opt.MapFrom(src => src.FK_CharityId))
            .ForMember(dest => dest.CharityName, opt => opt.MapFrom(src => src.Charity != null ? src.Charity.Name : null))
            .ForMember(dest => dest.Orphans, opt => opt.MapFrom(src =>
                src.OrphanReports.Where(r => !r.IsDeleted).Select(r => new OutgoingOrphanDto
                {
                    OrphanId = r.OrphanId,
                    Code = r.Orphan != null ? r.Orphan.Code : string.Empty,
                    FullName = r.Orphan != null ? r.Orphan.FullName : string.Empty
                })));

        CreateMap<Outgoing, OutgoingListDto>()
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.HasReply, opt => opt.MapFrom(src => src.IncomingId != null))
            .ForMember(dest => dest.IncomingLetterNumber, opt => opt.MapFrom(src => src.IncomingLetter != null ? src.IncomingLetter.LetterNumber : null))
            .ForMember(dest => dest.UploadedFileId, opt => opt.MapFrom(src => src.UploadedFileId))
            .ForMember(dest => dest.UploadedFileName, opt => opt.MapFrom(src => src.UploadedFile != null ? src.UploadedFile.FileName : null));
    }
}
