//using Framework.Core.SharedServices.Dto;
using System;

namespace Framework.Identity.Data.Dtos
{
    public class UserRegister
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string[] RoleNames { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }



        public string EmployeeNumber { get; set; }
        public int UserTypeId { get; set; }




        public string? Name { get; set; }
        public DateTime? BirthDate { get; set; }
        public int? GenderId { get; set; }
        public string? Nationality { get; set; }
        
        public int? PassportNum { get; set; }
        public DateTime? Passport_IssueDate { get; set; }
        public DateTime? Passport_ExpiryDate { get; set; }

        public int? UnifiedNumber { get; set; }

        public int? PersonalNumber { get; set; }
        public int? EmiratesIDNum { get; set; }
        public DateTime? EmiratesID_ExpiryDate { get; set; }

        public int? ResidencyNum { get; set; }
        public DateTime? Residency_ExpiryDate { get; set; }


        public string? Phone { get; set; }
        public string? Address { get; set; }


        public int? eSignatureNum { get; set; }


        public decimal BasicSalary { get; set; }
        public decimal TotalSalary { get; set; }
        public string? Position { get; set; }
        public string? JobTitle { get; set; }
        public DateTime? JobStartDate { get; set; }

        public DateTime? LaborCard_ExpiryDate { get; set; }
        public int? LaborCardNumber { get; set; }
        public int? LaborCardTypeId { get; set; }

        public int? EmployeeTypeId { get; set; }

        public decimal? Fines { get; set; }
        //public AttachmentDto EmployeePhoto { get; set; }

    }
}