using Framework.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Identity.Data.Dtos
{
    public class UserDto 
    {
        public Guid? Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool EmailConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public string NormalizedUserName { get; set; }
        public string NormalizedEmail { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string SecurityStamp { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public string ConcurrencyStamp { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UserTypeId { get; set; }
        public string PreferredNotificationLanguage { get; set; }
        public string? CurrentToken { get; set; }
        public List<string> RoleNames { get; set; } = new List<string>();
        public string RoleNameAsTxt => RoleNames != null && RoleNames.Any()
    ? string.Join(", ", RoleNames)
    : string.Empty;

        public List<LookupBaseDto<string>> Roles { get; set; } = new List<LookupBaseDto<string>>();
        public bool IsExternalUser { get; set; }
        public int? AgancyId { get; set; }

        public List<UserRolesDto> UserRoles { get; set; }

    }
}