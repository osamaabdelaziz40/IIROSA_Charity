using System;

namespace Framework.Identity.Data.Dtos
{
    public class ClaimUserRoleDto
    {
        public Guid? RoleId { get; set; }
        public string RoleName { get; set; }
    }
}