using AutoMapper;
using Framework.Core.AutoMapper;
using Framework.Core.Data;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Identity.Data
{
    public class IdentityAutoMapperProfile : Profile, IMapperProfile
    {
        public IdentityAutoMapperProfile()
        {
            // Explicit bidirectional mapping between ApplicationUser and UserDto
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.RoleNames, opt => opt.MapFrom(src =>
                    src.UserRoles != null ? src.UserRoles.Select(ur => ur.Role.Name).ToList() : new List<string>()))
                .ForMember(dest => dest.Roles, opt => opt.MapFrom(src =>
                    src.UserRoles != null ? src.UserRoles.Select(ur => new LookupBaseDto<string>
                    {
                        Id = ur.Role.Name,
                        Name = ur.Role.DisplayNameEn ?? ur.Role.Name
                    }).ToList() : new List<LookupBaseDto<string>>()))
                .ReverseMap()
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore()); // Ignore complex navigation property in DTO

            // Explicit mapping from UserDto to ApplicationUser (for updates)
            CreateMap<UserDto, ApplicationUser>()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id.HasValue))
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName != null ? src.UserName.ToUpperInvariant() : null))
                .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email != null ? src.Email.ToUpperInvariant() : null))
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore());

            CreateMap<UserUpdateDto, UserDto>().ReverseMap();
            CreateMap<ApplicationRole, RoleDto>().ReverseMap();
            CreateMap<ApplicationUserRoles, UserRolesDto>().ReverseMap();
            CreateMap<IdentityUserToken<Guid>, UserTokensDto>().ReverseMap();
        }

        public int Order { get; set; } = 1;
    }
}
