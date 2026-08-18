using System;

namespace Framework.Identity.Data.Dtos
{
    public class RoleManagementInsertResultDto
    {
        public Guid? InsertedId { get; set; }
        public string VerificationMSG { get; set; }
        public bool Success { get; set; }
    }
}