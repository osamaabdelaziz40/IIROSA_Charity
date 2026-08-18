using Framework.Core.AutoMapper;
using Framework.Core.Data;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Helper;
using Framework.Identity.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services
{
    public class UserRoleAppService
    {
        private readonly UserRolesRepository _userRolesRepository;

        public UserRoleAppService(UserRolesRepository userRolesRepository)
        {
            _userRolesRepository = userRolesRepository;
        }

        public async Task<Guid?> InsertAsync(UserRolesDto userRole, bool autoSave = false)
        {
            var entity = userRole.MapTo<ApplicationUserRoles>();
            var insertedEntity = await _userRolesRepository.InsertAsync(entity, autoSave);
            if (insertedEntity != null)
            {
                return insertedEntity.Id;
            }
            return null;
        }

        public async Task<Guid?> UpdateAsync(Guid userId, Guid roleId)
        {
            var oldData = await _userRolesRepository.TableNoTracking.Where(s => s.UserId == userId).FirstOrDefaultAsync();
            if (oldData == null)
            {
                return Guid.Empty;
            }
            oldData.RoleId = roleId;
            var updatedItem = await _userRolesRepository.UpdateAsync(oldData, true);
            return updatedItem.UserId;
        }

        public async Task InsertRangeAsync(List<UserRolesDto> userRoles, bool autoSave = false)
        {
            try
            {
                var userRolesList = userRoles.MapTo<List<ApplicationUserRoles>>();
                await _userRolesRepository.InsertRangeAsync(userRolesList, autoSave);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Guid?> ChangeRoleStatusForUserAsync(Guid userId, Guid roleId, bool isActive)
        {
            var oldData = await _userRolesRepository.TableNoTracking.Where(s => s.UserId == userId && s.RoleId == roleId).FirstOrDefaultAsync();
            if (oldData == null)
            {
                return Guid.Empty;
            }
            oldData.IsActive = isActive;
            var updatedItem = await _userRolesRepository.UpdateAsync(oldData, true);
            return updatedItem.UserId;
        }

        public async Task<RoleManagementDeleteResultDto> DeleteAsync(Guid id)
        {
            RoleManagementDeleteResultDto objResult = new RoleManagementDeleteResultDto();
            var oldData = await _userRolesRepository.TableNoTracking.Where(s => s.Id == id).FirstOrDefaultAsync();
            if (oldData != null)
            {
                if (oldData.RoleId == RoleHelper.CentralOperationsGM || oldData.RoleId == RoleHelper.DebtDepartmentManager || oldData.RoleId == RoleHelper.AuctionManager || oldData.RoleId == RoleHelper.CommitmentOfficer)
                {
                    var IsAnotherUserExist = await _userRolesRepository.TableNoTracking.Where(s => s.RoleId == oldData.RoleId && s.UserId != oldData.UserId).AnyAsync();
                    if (!IsAnotherUserExist)
                    {
                        objResult.DeleteStatus = false;
                        objResult.VerificationMSG = "Resources.SharedResources.DeleteApprovalUserError";
                        return objResult;
                    }
                }
                if (oldData.RoleId == RoleHelper.PortSupervisor)
                {
                    var IsAnotherUserExist = await _userRolesRepository.TableNoTracking.Where(s => s.RoleId == oldData.RoleId && s.UserId != oldData.UserId).AnyAsync();
                    if (!IsAnotherUserExist)
                    {
                        objResult.DeleteStatus = false;
                        objResult.VerificationMSG = "Resources.SharedResources.DeletePortSupervisorError";
                        return objResult;
                    }
                }
                if (oldData.RoleId == RoleHelper.CentralOperationsOfficer)
                {
                    var IsAnotherUserExist = await _userRolesRepository.TableNoTracking.Where(s => s.RoleId == oldData.RoleId && s.UserId != oldData.UserId).AnyAsync();
                    if (!IsAnotherUserExist)
                    {
                        objResult.DeleteStatus = false;
                        objResult.VerificationMSG = "Resources.SharedResources.DeleteCentralOperationsOfficerError";
                        return objResult;
                    }
                }
                if (oldData.RoleId == RoleHelper.PortSupervisor)
                {
                    var IsAnotherUserExist = await _userRolesRepository.TableNoTracking.Where(s => s.RoleId == oldData.RoleId && s.UserId != oldData.UserId).AnyAsync();
                    if (!IsAnotherUserExist)
                    {
                        objResult.DeleteStatus = false;
                        objResult.VerificationMSG = "Resources.SharedResources.PortSupervisorError";
                        return objResult;
                    }
                }
                objResult.DeleteStatus = await _userRolesRepository.DeleteAsync(s => s.Id == id, true);
            }
            else
            {
                objResult.DeleteStatus = false;
            }
            return objResult;
        }

        public async Task<UserRolesSearchDto> GetList(UserRolesSearchDto model)
        {
            var filters = new List<Expression<Func<ApplicationUserRoles, bool>>>();
            filters.Add(q => q.IsActive != false);
            if (model.IsApprovalRole.HasValue)
            {
                if (model.IsApprovalRole.Value)
                {
                    filters.Add(q => q.RoleId == RoleHelper.CentralOperationsGM || q.RoleId == RoleHelper.AuctionManager || q.RoleId == RoleHelper.CommitmentOfficer || q.RoleId == RoleHelper.DebtDepartmentManager);
                }
            }
            if (model.RoleId.HasValue)
            {
                filters.Add(q => q.RoleId == model.RoleId);
            }
            if (model.UserId.HasValue)
            {
                filters.Add(q => q.UserId == model.UserId);
            }
            model.PageSize = 15;//_appSettingsService.DefaultPagerPageSize;
            var result = _userRolesRepository
                  .SearchAndSelectWithFilters
                  (
                  b => b.OrderBy(a => a.CreatedOn),
                  a => a.MapTo<UserRolesDto>(),
                  filters
                  );
            // get paged list
            var results = result.GetPaged(
                o => o.RoleId, // order by
                model.IsDescending, // ascending (based on customer comments)
                model.PageNumber, // page number
                model.PageSize.Value);

            // bind items to search results
            model.Items = new StaticPagedList<UserRolesDto>(
                results,
                results.PageNumber,
                15,
                results.TotalItemCount);

            model.TotalItemsCount = model.Items.TotalItemCount;
            return await Task.FromResult(model);
        }

        public void DeleteByUserIdRoleId(Guid userId, Guid roleId)
        {
            var userRoles = _userRolesRepository.TableNoTracking.Where(s => s.UserId == userId && s.RoleId == roleId).ToList();
            _userRolesRepository.DeleteRange(userRoles, true);
        }

        public void DeleteByUserId(Guid userId)
        {
            var userRoles = _userRolesRepository.TableNoTracking.Where(s => s.UserId == userId).ToList();
            _userRolesRepository.DeleteRange(userRoles, true);
        }
    }
}