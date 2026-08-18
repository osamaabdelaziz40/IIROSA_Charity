using System;

namespace Framework.Identity.Data.Dtos
{
    public class UserTokensDto
    {
        public Guid UserId { get; set; }
        public string LoginProvider { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}