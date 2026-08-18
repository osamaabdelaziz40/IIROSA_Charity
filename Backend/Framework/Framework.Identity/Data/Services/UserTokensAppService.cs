using Framework.Core.AutoMapper;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services
{
    public class UserTokensAppService
    {
        private readonly UserTokensRepository _userTokensRepository;

        public UserTokensAppService(UserTokensRepository userTokensRepository)
        {
            _userTokensRepository = userTokensRepository;
        }

        public async Task<UserTokensDto> GetUserToken(string username, string jtiKey)
        {
            var userTokensObj = await _userTokensRepository.TableNoTracking.Where(q => q.Name == username && q.LoginProvider == jtiKey).FirstOrDefaultAsync();
            return userTokensObj.MapTo<UserTokensDto>();
        }

        public async Task<Guid> InsertAsync(UserTokensDto userTokensDto)
        {
            var userToken = userTokensDto.MapTo<IdentityUserToken<Guid>>();
            var insertedEntiry = await _userTokensRepository.InsertAsync(userToken, true);
            return insertedEntiry.UserId;
        }

        public async Task<bool> DeleteAsync(UserTokensDto userTokensDto)
        {
            var oldData = await _userTokensRepository.TableNoTracking.Where(s => s.UserId == userTokensDto.UserId && s.LoginProvider == userTokensDto.LoginProvider).FirstOrDefaultAsync();
            if (oldData != null)
            {
                return await _userTokensRepository.DeleteAsync(s => s.UserId == userTokensDto.UserId && s.LoginProvider == userTokensDto.LoginProvider, true);
            }
            else
            {
                return false;
            }
        }
    }
}