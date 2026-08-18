using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Dtos
{
    public class UpdatePreferredNotificationLanguageDTO
    {
        public Guid UserId { get; set; }
        public string LangKey { get; set; }
    }
}
