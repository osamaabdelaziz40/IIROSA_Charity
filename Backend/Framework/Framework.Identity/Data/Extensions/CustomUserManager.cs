//using Microsoft.AspNetCore.Identity;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Framework.Identity.Data.Extensions
//{
//    public class CustomUserManager<TUser> : UserManager<TUser> where TUser : class
//    {
//        public CustomUserManager(IUserStore<TUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<TUser> passwordHasher, IEnumerable<IUserValidator<TUser>> userValidators, IEnumerable<IPasswordValidator<TUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<TUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
//        {
//        }

//        public override async Task<IdentityResult> CreateAsync(TUser user)
//        {
//            ThrowIfDisposed();
            
//            var result = await ValidateUserAsync(user);
//            if (!result.Succeeded)
//            {
//                return result;
//            }
//            if (Options.Lockout.AllowedForNewUsers && SupportsUserLockout)
//            {
//               // await GetUserLockoutStore().SetLockoutEnabledAsync(user, true, CancellationToken);
//            }
//            await UpdateNormalizedUserNameAsync(user);
//            await UpdateNormalizedEmailAsync(user);

//            return await Store.CreateAsync(user, CancellationToken);
//        }
//    }
//}
