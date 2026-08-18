using Framework.Core.Extensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Framework.Identity.Data.Entities
{
    /// <summary>
    /// Represents a role in the identity system
    /// </summary>
    public class ApplicationRole : IdentityRole<Guid>
    {
        public string DisplayNameAr { get; set; }
        public string DisplayNameEn { get; set; }

        [Column(Order = 300)]
        public string CreatedBy { get; set; }

        [Column(Order = 301)]
        public DateTime CreatedOn { get; set; }

        [Column(Order = 302)]
        public string UpdatedBy { get; set; }

        [Column(Order = 303)]
        public DateTime? UpdatedOn { get; set; }
        public string? Email { get; set; }
        public ApplicationRole()
        { 
        
        }
        public ApplicationRole(Guid id, string displayNameAr, string displayNameEn, string name, string normalizedName, string createdBy, DateTime createdon, string? email=null)
        {
            Id = id.AgainstDefault(nameof(id));
            DisplayNameAr = displayNameAr.AgainstNullOrEmpty(nameof(displayNameAr));
            DisplayNameEn = displayNameEn.AgainstNullOrEmpty(nameof(id));
            Name = name.AgainstNullOrEmpty(nameof(id));
            NormalizedName = normalizedName.AgainstNullOrEmpty(nameof(id));
            CreatedBy = createdBy; 
            CreatedOn = createdon;
            ConcurrencyStamp = null;
            Email = email;
        }
    }
}