using Framework.Core.Data;
using Framework.Identity.Data.Entities;
using PagedList.Core;

namespace Framework.Identity.Data.Dtos
{
    public class RoleSearchDto : PagingDto
    {
        public string Name { get; set; }
        public string Group { get; set; }

        public new StaticPagedList<ApplicationRole> Items { get; set; }
    }
}