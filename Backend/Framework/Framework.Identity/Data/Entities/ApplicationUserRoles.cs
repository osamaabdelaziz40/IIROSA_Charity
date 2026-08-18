using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Framework.Identity.Data.Entities
{
    public class ApplicationUserRoles : IdentityUserRole<Guid>
    {
        public Guid Id { get; set; }
        public bool? IsActive { get; set; }

        [Column(Order = 300)]
        public string CreatedBy { get; set; }

        [Column(Order = 301)]
        public DateTime CreatedOn { get; set; }

        [Column(Order = 302)]
        public string UpdatedBy { get; set; }

        [Column(Order = 303)]
        public DateTime? UpdatedOn { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}