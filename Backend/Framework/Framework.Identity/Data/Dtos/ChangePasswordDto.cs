using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DGA.Raqmi.Application.Services.Users.Dtos
{
    public  class ChangePasswordDto
    {
        public string token { get; set;}
        public Guid? UserId { get; set;}
        public string OldPassword { get; set;}
        public string NewPassword { get; set;}
    }
}
