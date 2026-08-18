using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Identity.Data.Dtos
{
    public class UpdateProfileDto
    {
        [HiddenInput]
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string LangKey { get; set; }

    }
}
