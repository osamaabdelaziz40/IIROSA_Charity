using Framework.Core;
using Framework.Core.Extensions;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Framework.Identity.Data.Entities
{
    //test
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser()
        {

        }
        public ApplicationUser(string userName, string fullName, string email = null, bool isActive = true)
        {
            Check.NotNull(userName, nameof(userName));

            Id = Guid.NewGuid().AsSequentialGuid();
            UserName = userName.ToLower();
            FullName = fullName;
            NormalizedUserName = userName.ToUpperInvariant();
            Email = email?.ToLower();
            NormalizedEmail = email?.ToUpperInvariant();
            SecurityStamp = Guid.NewGuid().ToString();
            IsActive = isActive;
        }

        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public string CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? UserTypeId { get; set; }

        public string Otp { get; private set; }
        public string PreferredNotificationLanguage { get; set; } = "ar";
        public virtual ICollection<ApplicationUserRoles> UserRoles { get; set; }

        public int? AgancyId { get; set; }
        public string? CurrentToken { get; set; }

        /// <summary>
        /// The charity this user belongs to. Null for head-office users, who are not scoped to a
        /// single charity. Carried into the access token so tenancy can be enforced server-side.
        /// </summary>
        public Guid? CharityId { get; set; }

        /// <summary>
        /// The country this user operates in. Scopes head-office roles that are not bound to one
        /// charity but must not see other countries.
        /// </summary>
        public int? CountryId { get; set; }

        public void GenerateOtp()
        {
            Random random = new Random();
            int otpLength = 6;

            // Generate a random six-digit number
            int otpValue = random.Next((int)Math.Pow(10, otpLength - 1), (int)Math.Pow(10, otpLength));

            // Convert the number to a string and return it
            this.Otp=  otpValue.ToString();
        }
    }
}