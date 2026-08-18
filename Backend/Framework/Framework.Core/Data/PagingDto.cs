using Microsoft.AspNetCore.Mvc;
using PagedList.Core;

namespace Framework.Core.Data
{
    public abstract class PagingDto
    {
        public StaticPagedList<object> Items { get; set; }

        [HiddenInput]
        public int PageNumber { get; set; } = 1;

        public int? PageSize { get; set; } = int.MaxValue;

        public bool IsExport { get; set; } = false;
        [HiddenInput]
        public bool IsDescending { get; set; } = true;

        public bool IsSearchOpen { get; set; } = false;

        public string ReturnUrl { get; set; }
        public int TotalItemsCount { get; set; }
    }
}