using DGA.Raqmi.Application.Services.Users.Dtos;
using Framework.Core;
using Framework.Identity.Data.Dtos;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Services.Interfaces
{
    public interface IUserAppService
    {
        public string CurrentUserName { get; }
        public string CurrentUserEmail { get; }
 
        public Task<UserGridSearchDto> GetGridList(UserGridSearchDto model);
        public Task<List<UserDto>> GetUsersInRoles(List<string> roleNames);
       
        public Task<bool> DeleteAsync(Guid id);
        
        public Task<UserDto> FindByEmailAsync(string email);
        public Task<bool> UpdateEmailConfirmed(string email);
        public Task<ApiResponse< UserDto>> Register(UserRegister input);
        public Task<ApiResponse<UserDto>> Login(LoginDto model);
        //public Task<bool> VerifyOtp(VerifyOtpDto model,string username);

        public Task<bool> ChangePassword(string username,ChangePasswordDto changePassword);
    }
}