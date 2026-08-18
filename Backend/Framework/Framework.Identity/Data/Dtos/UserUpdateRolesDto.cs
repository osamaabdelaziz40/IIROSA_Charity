using System.ComponentModel.DataAnnotations;

namespace Framework.Identity.Data.Dtos
{
    public class UserUpdateRolesDto
    {
        [Required]
        public string[] RoleNames { get; set; }
    }
}