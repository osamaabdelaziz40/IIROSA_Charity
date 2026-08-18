using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Dtos
{
    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public Guid? PhotoId { get; set; }
        public string? imgSrc { get; set; }
    }
}
