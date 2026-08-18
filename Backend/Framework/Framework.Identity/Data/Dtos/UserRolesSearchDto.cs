using Framework.Core.Data;
using PagedList.Core;
using System;

namespace Framework.Identity.Data.Dtos
{
    public class UserRolesSearchDto : PagingDto
    {
        public Guid? UserId { get; set; }
        public Guid? RoleId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsApprovalRole { get; set; }
        public int TotalItemsCount { get; set; }
        public new StaticPagedList<UserRolesDto> Items { get; set; }
    }
}