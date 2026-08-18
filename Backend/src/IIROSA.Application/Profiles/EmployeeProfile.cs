using AutoMapper;
using IIROSA.Application.DTOs.EmployeeManagement;
using IIROSA.Domain.Entities;

namespace IIROSA.Application.Profiles;

/// <summary>
/// AutoMapper Profile for Employee entity
/// Maps between Employee entity and DTOs
/// </summary>
public class EmployeeProfile : Profile
{
    public EmployeeProfile()
    {
        // Entity to DTO mappings
        CreateMap<Employee, EmployeeListDto>()
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.NameAr ?? src.Department.NameEn : null))
            .ForMember(dest => dest.HireDate,
                opt => opt.MapFrom(src => src.HireDate ?? DateTime.MinValue))
            .ForMember(dest => dest.Roles,
                opt => opt.Ignore()); // Roles are loaded separately

        CreateMap<Employee, EmployeeDetailDto>()
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.NameAr ?? src.Department.NameEn : null))
            .ForMember(dest => dest.Roles,
                opt => opt.Ignore()); // Roles are loaded separately

        CreateMap<Employee, EmployeeExportDto>()
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.IsActive ? "Active" : "Inactive"))
            .ForMember(dest => dest.DepartmentName,
                opt => opt.MapFrom(src => src.Department != null ? src.Department.NameAr ?? src.Department.NameEn : null))
            .ForMember(dest => dest.CreatedOn,
                opt => opt.MapFrom(src => src.CreatedOn));

        // DTO to Entity mappings
        CreateMap<CreateEmployeeDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());

        CreateMap<UpdateEmployeeDto, Employee>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FK_UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Department, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedOn, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.Events, opt => opt.Ignore());
    }
}