using Framework.Core.Data;
using Framework.Core.SharedServices.Entities;
using PagedList.Core;
using System;

namespace Framework.Core.SharedServices.Dto
{
    public class LogSearchDto : PagingDto
    {
        public string LogLevel { get; set; }

        public string UserName { get; set; }

        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }

        public new StaticPagedList<Log> Items { get; set; }
    }
}