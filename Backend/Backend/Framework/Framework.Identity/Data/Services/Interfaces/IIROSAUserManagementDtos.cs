using System;
using System.Collections.Generic;

namespace Framework.Identity.Data.Services.Interfaces
{
    // NEW DTOs for IIROSA User Role Management (Only non-conflicting ones)

    public class UserDetailDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public string PhoneNumber { get; set; }
        public string PreferredNotificationLanguage { get; set; }
    }

    public class CreateUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
        public string Password { get; set; }
    }

    public class UpdateUserDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; }
    }

    public class UserFilterDto
    {
        public string SearchText { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class UserListResponseDto
    {
        public List<UserListItemDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class UserListItemDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
