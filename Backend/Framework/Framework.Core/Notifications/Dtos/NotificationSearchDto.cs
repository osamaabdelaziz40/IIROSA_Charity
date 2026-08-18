using Framework.Core.Data;
using Framework.Core.SharedServices.Dto;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Core.Notifications.Dtos
{
    public class NotificationSearchDto: PagingDto
    {
        
        public int? TotalItemsCount { get; set; }
        public RequestFilterObject? Filter { get; set; } = new RequestFilterObject();

        public new StaticPagedList<NotificationsLogDto>? Items { get; set; } = null;
    }
    public class RequestFilterObject
    {
        public bool? IsRead { get; set; }
    }

}
