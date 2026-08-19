using Framework.Core.Data;
using System.Collections.Generic;
using System;

namespace Framework.Identity.Data.Dtos
{
    public abstract class UserCreateOrUpdateDtoBase
    {
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string FullNameAr { get; set; }
        public string JobTitle { get; set; }
        public string Department { get; set; }

        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public bool LockoutEnabled { get; set; }

        public bool IsActive { get; set; }

        public string[] RoleNames { get; set; }
        public List<LookupBaseDto<string>> Roles { get; set; } = new List<LookupBaseDto<string>>();

        /// <summary>
        /// The charity this user belongs to. Null for head-office users. Set at creation time so
        /// the account can be scoped server-side from its first login; an account created without
        /// it cannot be tenancy-scoped at all.
        /// </summary>
        public Guid? CharityId { get; set; }

        /// <summary>
        /// The country this user operates in.
        /// </summary>
        public int? CountryId { get; set; }
    }
}