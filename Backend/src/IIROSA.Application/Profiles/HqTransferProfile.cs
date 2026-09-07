using AutoMapper;
using IIROSA.Application.DTOs.HqTransfers;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for the HqTransfer entity (UC-TRF-01…08).
///
/// The entity keeps its legacy `FK_`-prefixed columns (database unchanged); the DTOs use clean
/// names so the camelCase wire contract reads `countryId`, not `fK_CountryId`. FK pairs are
/// bridged with explicit ForMember maps; lookup names resolve `NameAr ?? NameEn` (Arabic primary).
/// </summary>
public class HqTransferProfile : Profile
{
    public HqTransferProfile()
    {
        // ========== Entity → DTO ==========

        // Grid row (UC-TRF-01 — §22.S.1's 11 data columns)
        CreateMap<HqTransfer, HqTransferListDto>()
            .ForMember(dest => dest.CountryName,
                opt => opt.MapFrom(src =>
                    src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src =>
                    src.Department != null ? src.Department.NameAr ?? src.Department.NameEn : null));

        // Detail (UC-TRF-02/03/04) — FK ids feed the edit form's patch step; the names come
        // from the repository's includes
        CreateMap<HqTransfer, HqTransferDetailDto>()
            .ForMember(dest => dest.CountryId, opt => opt.MapFrom(src => src.FK_CountryId))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.FK_DepartmentId))
            .ForMember(dest => dest.CountryName,
                opt => opt.MapFrom(src =>
                    src.Country != null ? src.Country.NameAr ?? src.Country.NameEn : null))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src =>
                    src.Department != null ? src.Department.NameAr ?? src.Department.NameEn : null));

        // ========== DTO → Entity ==========

        // Create (UC-TRF-02) — audit fields and navigations belong to the base/pipeline; the
        // service pins FK_CountryId to the caller's claim when their token carries one
        CreateMap<CreateHqTransferDto, HqTransfer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.FK_DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore());

        // Update (UC-TRF-04) — mapped ONTO the tracked entity (Map(source, destination)):
        // Id and audit fields stay untouched, the interceptor owns UpdatedOn/UpdatedBy
        CreateMap<UpdateHqTransferDto, HqTransfer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_CountryId, opt => opt.MapFrom(src => src.CountryId))
            .ForMember(dest => dest.FK_DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.Country, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore());

        // ========== Detail lines (UC-TRF-08) ==========

        // Grid row read — §22.S.3's line fields; FK/nav have no DTO counterpart
        CreateMap<HqTransferDetail, HqTransferDetailLineDto>();

        // Save — used for BOTH the add (Map<HqTransferDetail>(dto)) and the update onto the
        // tracked line (Map(dto, line)): the service owns the parent FK and the id; audit and
        // soft-delete belong to the base/pipeline
        CreateMap<SaveHqTransferDetailLineDto, HqTransferDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_HqTransferId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.HqTransfer, opt => opt.Ignore());
    }
}
