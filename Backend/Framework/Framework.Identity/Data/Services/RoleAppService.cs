using Framework.Core.Angular;
using Framework.Core.AutoMapper;
using Framework.Core.Extensions;
using Framework.Core.Globalization;
using Framework.Core.SharedServices.Services;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Repositories;
using Framework.Identity.Data.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services
{
    public class RoleAppService : IRoleAppService
    {
        //private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly RoleRepository _roleRepository;
        private readonly AppSettingsService _appSettingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RoleAppService(
            //RoleManager<ApplicationRole> roleManager,
           RoleRepository roleRepository, AppSettingsService appSettingsService,
           IHttpContextAccessor httpContextAccessor)
        {
            //_roleManager = roleManager;
            _roleRepository = roleRepository;
            _appSettingsService = appSettingsService;
            _httpContextAccessor = httpContextAccessor;
        }
        public string CurrentUserName => _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(s => s.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value?.To<string>();

        public async Task<IEnumerable<SelectListItem>> List()
        {
            return await _roleRepository.TableNoTracking
              .Select(s => new SelectListItem
              {
                  Text = CultureHelper.IsArabic ? s.DisplayNameAr : s.DisplayNameEn,
                  Value = s.Name,
              }).ToListAsync();
        }

        //public async Task<RoleDto> GetAsync(Guid id)
        //{
        //    var result = await _roleManager.FindByIdAsync(id.ToString());
        //    return result.MapTo<RoleDto>();

        //}
        public async Task<List<RoleDto>> GetAllAsync()
        {
            var result = await _roleRepository.TableNoTracking.ToListAsync();
            return result.MapTo<List<RoleDto>>();
        }
        public async Task<List<SelectListItem<Guid>>> GetRolesList()
        {
            var roles = await _roleRepository.TableNoTracking.Select(p => new SelectListItem<Guid>
            {
                Value = p.Id,
                NameAr = p.DisplayNameAr,
                NameEn = p.DisplayNameEn,
            }).ToListAsync();

            return roles;
        }
        public async Task<string> GetRoleNameByRoleId(Guid roleId)
        {
            return await _roleRepository.TableNoTracking.Where(q => q.Id == roleId).Select(q => CultureHelper.IsArabic ? q.DisplayNameAr : q.DisplayNameEn).FirstOrDefaultAsync();
        }
        public async Task<string> GetRoleByRoleId(Guid roleId)
        {
            return await _roleRepository.TableNoTracking.Where(q => q.Id == roleId).Select(q => q.Name).FirstOrDefaultAsync();
        }
        public async Task<RoleDto> FindByRoleNameAsync(string roleName)
        {
            try
            {
                var result = await _roleRepository.TableNoTracking.Where(x => x.Name == roleName).FirstOrDefaultAsync();
                return result.MapTo<RoleDto>();
            }

            catch (Exception e)
            {
                throw;
            }
        }

        public RoleSearchDto GetList(RoleSearchDto model)
        {

            var filters = new List<Expression<Func<ApplicationRole, bool>>>();


            if (!model.Name.IsNullOrEmpty())
            {
                Expression<Func<ApplicationRole, bool>>
                    filter = r => r.DisplayNameAr.ToLower().Contains(model.Name) || r.DisplayNameEn.ToLower().Contains(model.Name);
                filters.Add(filter);
            }

            Func<IQueryable<ApplicationRole>, IOrderedQueryable<ApplicationRole>> orderBy;
            if (model.IsDescending)
            {
                orderBy = a => a.OrderByDescending(b => b.Name);
            }
            else
            {
                orderBy = a => a.OrderBy(b => b.Name);
            }

            model.PageSize = _appSettingsService.DefaultPagerPageSize;

            var result = _roleRepository.SearchWithFilters
                (
                model.PageNumber,
                model.IsExport ? _appSettingsService.ExportNoOfItems : model.PageSize.Value,
                orderBy,
                filters
                );

            model.Items =
                new StaticPagedList<ApplicationRole>(
                    result,
                    result.PageNumber,
                    result.PageSize,
                    result.TotalItemCount);

            return model;


        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var user = await _roleRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (user != null)
                return await _roleRepository.DeleteAsync(u => u.Id == id, true);
            else
                return false;
        }

        public async Task<List<string>> GetRoleNamesByIds(List<Guid> ids)
        {
            var roles = await _roleRepository.TableNoTracking.Where(s => ids.Contains(s.Id)).Select(s => CultureHelper.IsArabic ? s.DisplayNameAr : s.DisplayNameEn).ToListAsync();
            return roles;

        }

        public async Task<List<RoleDto>> GetRolesByIds(List<Guid> ids)
        {
            var roles = await _roleRepository.TableNoTracking.Where(s => ids.Contains(s.Id)).ToListAsync();
            var result = roles.MapTo<List<RoleDto>>();
            return result;
        }
        public async Task<RoleManagementInsertResultDto> InsertAsync(RoleDto role)
        {
            RoleManagementInsertResultDto objResult = new RoleManagementInsertResultDto();
            var NormalizedName = role.Name.ToUpper();
            var UserObj = await _roleRepository.FindByNameAsync(NormalizedName);
            if (UserObj != null)
            {
                objResult.InsertedId = Guid.Empty;
                objResult.VerificationMSG = "RoleAlreadyExists";
                return objResult;
            }
            role.NormalizedName = NormalizedName;
            var entity = role.MapTo<ApplicationRole>();
            entity.CreatedBy = CurrentUserName;
            var resultRole = await _roleRepository.InsertAsync(entity, true);
            objResult.InsertedId = resultRole.Id;

            return objResult;
        }
        public async Task<RoleDto> GetRoleByIdAsync(Guid id)
        {
            var role = await _roleRepository.TableNoTracking.Where(x => x.Id == id).FirstOrDefaultAsync();
            var result = role.MapTo<RoleDto>();
            return result;
        }
        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            var role = await _roleRepository.TableNoTracking.FirstOrDefaultAsync(a => a.Name == roleName);
            var result = role.MapTo<RoleDto>();
            return result;
        }
        public async Task<Guid> UpdateAsync(RoleDto input)
        {
            var role = await _roleRepository.GetByIdAsync(input.Id);
            if (!role.Id.ToString().IsNotNullOrEmpty())
            {
                return Guid.Empty;
            }
            role.Name = input.Name;
            role.DisplayNameAr = input.DisplayNameAr;
            role.DisplayNameEn = input.DisplayNameEn;

            await _roleRepository.UpdateAsync(role, true);

            return role.Id;
        }

        // NEW METHODS for IIROSA User Role Management
        public async Task<RoleDetailDto> GetRoleDetailAsync(Guid id)
        {
            var role = await _roleRepository.TableNoTracking.FirstOrDefaultAsync(r => r.Id == id);
            if (role == null) return null;

            var usersInRole = await _roleRepository.TableNoTracking
                .Join(_roleRepository.Context.Set<ApplicationUserRoles>(),
                    r => r.Id,
                    ur => ur.RoleId,
                    (r, ur) => new { Role = r, UserRole = ur })
                .Join(_roleRepository.Context.Set<ApplicationUser>(),
                    combined => combined.UserRole.UserId,
                    u => u.Id,
                    (combined, u) => u)
                .Where(u => u.IsActive)
                .ToListAsync();

            return new RoleDetailDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayNameAr = role.DisplayNameAr,
                DisplayNameEn = role.DisplayNameEn,
                CreatedOn = role.CreatedOn,
                UpdatedOn = role.UpdatedOn,
                UserCount = usersInRole.Count,
                Users = usersInRole.Take(10).Select(u => new UserBasicDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    IsActive = u.IsActive
                }).ToList()
            };
        }

        public async Task<List<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            var role = await _roleRepository.TableNoTracking.FirstOrDefaultAsync(r => r.Name == roleName);
            if (role == null) return new List<ApplicationUser>();

            var userIds = await _roleRepository.Context.Set<ApplicationUserRoles>()
                .Where(ur => ur.RoleId == role.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            return await _roleRepository.Context.Set<ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        public async Task<int> GetUserCountInRoleAsync(string roleName)
        {
            var users = await GetUsersInRoleAsync(roleName);
            return users.Count;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleRepository.TableNoTracking.AnyAsync(r => r.Name == roleName);
        }

        public async Task<RoleManagementInsertResultDto> CreateRoleAsync(CreateRoleDto role)
        {
            var result = new RoleManagementInsertResultDto();

            // Check if role already exists
            if (await RoleExistsAsync(role.Name))
            {
                result.InsertedId = Guid.Empty;
                result.VerificationMSG = "RoleAlreadyExists";
                result.Success = false;
                return result;
            }

            var entity = new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = role.Name,
                NormalizedName = role.Name.ToUpperInvariant(),
                DisplayNameAr = role.DisplayNameAr,
                DisplayNameEn = role.DisplayNameEn,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                CreatedBy = CurrentUserName,
                CreatedOn = DateTime.UtcNow
            };

            await _roleRepository.InsertAsync(entity, true);

            result.InsertedId = entity.Id;
            result.Success = true;
            return result;
        }

        public async Task<bool> UpdateRoleDetailAsync(Guid id, UpdateRoleDto role)
        {
            var existingRole = await _roleRepository.GetByIdAsync(id);
            if (existingRole == null) return false;

            if (!string.IsNullOrEmpty(role.DisplayNameAr))
            {
                existingRole.DisplayNameAr = role.DisplayNameAr;
            }

            if (!string.IsNullOrEmpty(role.DisplayNameEn))
            {
                existingRole.DisplayNameEn = role.DisplayNameEn;
            }

            existingRole.UpdatedBy = CurrentUserName;
            existingRole.UpdatedOn = DateTime.UtcNow;

            await _roleRepository.UpdateAsync(existingRole, true);
            return true;
        }

        public async Task<bool> CanDeleteRoleAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return false;

            // Check if role has users
            var userCount = await GetUserCountInRoleAsync(role.Name);
            if (userCount > 0) return false;

            // Prevent deletion of IIROSA core roles
            var coreRoles = new[] { "SuperAdmin", "Admin", "Charity", "Accountant", "FinancialOfficer" };
            if (coreRoles.Contains(role.Name)) return false;

            return true;
        }
    }
}
