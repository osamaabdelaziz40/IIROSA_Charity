using Framework.Core.Data;
using Framework.Core.Helper;
using Framework.Identity.Data.Entities;
using Framework.Identity.Data.Extensions;
using Framework.Identity.Data.Helper;
using Framework.Identity.Data.Seed;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Identity.Data
{
    public class AppIdentityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRoles, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>, IBaseDbContext
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options, IHttpContextAccessor httpContextAccessor , IIdentityTokenManager identityTokenManager)
            : base(options)
        {
            CurrentUserName = httpContextAccessor?.HttpContext?.User?.Identity?.Name;
            var token = httpContextAccessor?.HttpContext?.Request.Headers.Authorization;
            if(token.HasValue)
            {
                CurrentUserName =string.IsNullOrEmpty(token.Value)?"":  identityTokenManager.GetCurrentUserName(token.Value.ToString());

            }
        }

        public string CurrentUserName { get; set; }
        public static string Schema { get; set; } = "identity";

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ConfigureIdentity(options =>
            {
                options.Schema = Schema;
            });

            builder.ApplyConfiguration(new RoleConfiguration());
            // ApplicationUserRoles keeps the legacy surrogate Id PK (matches the DB and the
            // snapshot). Identity's 2-value FindAsync(userId, roleId) only works against the
            // composite key, which is why AppUserStore is registered in
            // IdentityRegisterDependencies to override FindUserRoleAsync/CreateUserRole.
            builder.Entity<ApplicationUserRoles>(b =>
            {
                // Primary key is surrogate Id
                b.HasKey(ur => ur.Id);

                // Foreign key to ApplicationUser via UserId
                b.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();

                // Foreign key to ApplicationRole via RoleId
                b.HasOne(ur => ur.Role)
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            //builder.ApplyConfiguration(new AdminConfiguration());
            //builder.ApplyConfiguration(new UsersWithRolesConfig());
        }

        public override int SaveChanges()
        {
            try
            {
                ChangeTracker.SetShadowProperties(CurrentUserName);
                ChangeTracker.Validate();

                return base.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                ChangeTracker.SetShadowProperties(CurrentUserName);
                ChangeTracker.Validate();

                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
            finally
            {
                ChangeTracker.AutoDetectChangesEnabled = true;
            }
        }

        public Task<int> SaveChangesWithAuditAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        
    }
}