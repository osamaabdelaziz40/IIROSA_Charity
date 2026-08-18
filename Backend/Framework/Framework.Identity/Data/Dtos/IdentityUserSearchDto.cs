using Framework.Core.Data;
using Framework.Identity.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using PagedList.Core;
using System.Collections.Generic;

namespace Framework.Identity.Data.Dtos
{
    public class IdentityUserSearchDto : PagingDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool? IsActive { get; set; }
        public string IdentityNo { get; set; }
        public bool IsExternalUser { get; set; } = false;
        public new StaticPagedList<ApplicationUser> Items { get; set; }
        public List<ApplicationUser> ExportedItems { get; set; }
    }

    public class UserGridSearchDto : PagingDto
    {
        public string UserTxtSearch { get; set; }
        public string RoleName { get; set; }
        public List<SelectListItem> Role { get; set; }
        public bool? IsActive { get; set; }
        public int TotalItemsCount { get; set; }
        public new StaticPagedList<ApplicationUser> Items { get; set; }
        public List<ApplicationUser> ExportedItems { get; set; }
    }
}