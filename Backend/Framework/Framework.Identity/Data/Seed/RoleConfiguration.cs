using Framework.Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Framework.Identity.Data.Seed
{
    public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> builder)
        {
            builder.HasData(
                 new List<ApplicationRole>()
                {
                  //admin Role
                  new ApplicationRole(Guid.Parse("d29995f4-abeb-468c-889b-6d6d392cb852"), "مدير النظام", "admin", "admin", "admin", "", new DateTime(2023, 7, 1) ),  

                  //Emoloyee Role
                  new ApplicationRole(Guid.Parse("ECCD727D-A07D-42F3-B817-535DBD6048FB"), "موظف", "Employee", "Employee", "Employee", "admin", new DateTime(2023, 7, 1) ),
                  

                  //DataEntry Role
                  new ApplicationRole(Guid.Parse("2821231E-6ABC-4A3D-AFB6-EB7495A64B42"), "مدخل بيانات", "DataEntry", "DataEntry", "DataEntry", "admin", new DateTime(2025, 1, 23) ),

                 });
        }
    }
}