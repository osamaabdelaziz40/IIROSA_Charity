using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Extensions
{
    public class AppUserStore : UserStore<
        ApplicationUser,
        ApplicationRole,
        AppIdentityDbContext,
        Guid,
        IdentityUserClaim<Guid>,
        ApplicationUserRoles,
        IdentityUserLogin<Guid>,
        IdentityUserToken<Guid>,
        IdentityRoleClaim<Guid>>
    {
        public AppUserStore(AppIdentityDbContext context, IdentityErrorDescriber? describer = null)
            : base(context, describer)
        {
        }

        protected override Task<ApplicationUserRoles?> FindUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken)
        {
            return Context.Set<ApplicationUserRoles>().FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
        }

        protected override ApplicationUserRoles CreateUserRole(ApplicationUser user, ApplicationRole role)
        {
            return new ApplicationUserRoles
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = role.Id,
                CreatedBy = Context.CurrentUserName ?? "System",
                CreatedOn = DateTime.UtcNow,
                IsActive = true
            };
        }
    }
}
