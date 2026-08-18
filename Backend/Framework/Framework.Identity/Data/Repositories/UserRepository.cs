using Framework.Core;
using Framework.Core.Data;
using Framework.Core.Data.Repositories;
using Framework.Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Repositories
{
    public class UserRepository : RepositoryBase<AppIdentityDbContext, ApplicationUser>
    {
        public UserRepository(AppIdentityDbContext dbContext)
            : base(dbContext)
        {
        }

        public virtual async Task<ApplicationUser> FindByUserNameAsync(string userName)
        {
            return await TableNoTracking
                .FirstOrDefaultAsync(
                    u => u.UserName == userName);
        }

        public virtual async Task<ApplicationUser> FindByUserIdAsync(Guid id)
        {
            return await TableNoTracking
                .FirstOrDefaultAsync(
                    u => u.Id == id);
        }

        public virtual async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return await DbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public virtual async Task<PagedList<ApplicationUser>> GetListAsync<T, TOrderBy>(
            Expression<Func<T, TOrderBy>> orderBy = null,
            bool isDescending = true,
            int pageNum = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            return await TableNoTracking.GetPagedAsync(i => i.CreatedOn, isDescending, pageNum, pageSize);
        }

        public virtual async Task<List<ApplicationRole>> GetRolesAsync(
            Guid id,
            bool includeDetails = false)
        {
            var query = from userRole in DbContext.Set<ApplicationUserRoles>()
                        join role in DbContext.Roles on userRole.RoleId equals role.Id
                        where userRole.UserId == id
                        orderby userRole.CreatedOn
                        select role;

            return await query.ToListAsync();
        }

        public virtual async Task<List<ApplicationRole>> GetRolesByUsernameAsync(
            string name,
            bool includeDetails = false)
        {
            var query = from userRole in DbContext.Set<ApplicationUserRoles>()
                        join role in DbContext.Roles on userRole.RoleId equals role.Id
                        join user in DbContext.Users on userRole.UserId equals user.Id
                        where userRole.UserId == user.Id && user.UserName == name
                        select role;

            return await query.ToListAsync();
        }
    }
}