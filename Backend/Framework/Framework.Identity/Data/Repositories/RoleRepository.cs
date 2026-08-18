using Framework.Core.Data.Repositories;
using Framework.Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Repositories
{
    public class RoleRepository : RepositoryBase<AppIdentityDbContext, ApplicationRole>
    {
        private readonly AppIdentityDbContext _context;

        public RoleRepository(AppIdentityDbContext dbContext)
            : base(dbContext)
        {
            _context = dbContext;
        }

        // Add Context property for accessing other entities
        public AppIdentityDbContext Context => _context;

        public virtual async Task<ApplicationRole> FindByNameAsync(string name)
        {
            return await TableNoTracking.FirstOrDefaultAsync(r => r.Name.ToLower() == name.ToLower());
        }

        public ApplicationRole GetById(Guid id)
        {
            return TableNoTracking.FirstOrDefaultAsync(r => r.Id == id).Result;
        }

        public async Task<ApplicationRole> GetByIdAsync(Guid id)
        {
            return await TableNoTracking.FirstOrDefaultAsync(r => r.Id == id);
        }

        public virtual async Task<List<ApplicationRole>> GetListAsync()
        {
            return await TableNoTracking.ToListAsync();
        }
    }
}