using Microsoft.AspNetCore.Mvc;
using System;

namespace Framework.Identity.Data.Dtos
{
    public class ChangePasswordAdminDto
    {
        [HiddenInput]
        public Guid UserId { get; set; }

        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}