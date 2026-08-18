using Microsoft.AspNetCore.Mvc;
using System;

namespace Framework.Identity.Data.Dtos
{
    public class UserUpdateDto : UserCreateOrUpdateDtoBase
    {
        [HiddenInput]
        public Guid Id { get; set; }
    }
}