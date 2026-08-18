using Framework.Core.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Repositories
{
    public class UserTokensRepository : RepositoryBase<AppIdentityDbContext, IdentityUserToken<Guid>>
    {
        public UserTokensRepository(AppIdentityDbContext dbContext) : base(dbContext)
        {
        }

        public virtual async Task<List<IdentityUserToken<Guid>>> GetListAsync()
        {
            return await TableNoTracking.ToListAsync();
        }
    }
}