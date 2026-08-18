//using Framework.Identity.Data.Entities;
//using Framework.Identity.Data.Helper;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;

//namespace Framework.Identity.Data.Seed
//{
//    public class UsersWithRolesConfig : IEntityTypeConfiguration<ApplicationUserRoles>
//    {
//        private readonly Guid _adminId = new Guid("B22698B8-42A2-4115-9631-1C2D1E2AC5F7");
//        private readonly Guid _individualId = new Guid("AAAA4BA6-94D9-4395-BB1C-9D54939E70EF");
//        private readonly Guid _companyId = new Guid("ED6D642F-50ED-4E28-BDE5-03B4C88F9387");
//        private readonly Guid _AuctionSupervisorId = new Guid("DB262BE8-1A1A-4910-99BA-65D89D8FD90E");
//        private readonly Guid _centralOperationsOfficerId = new Guid("F1F088FC-E2CB-4AC4-BF04-12A0800415A6");
//        private readonly Guid _headOfSectorId = new Guid("3BA0E4C9-5886-4ABD-A497-B60492DFDC06");
//        private readonly Guid _SectorHead1Id = new Guid("b16d4f18-7336-4c7b-a966-b2be2f1e0cd4");
//        private readonly Guid _CentralOperationGMId = new Guid("9c2d3cfa-a0fb-4cd0-8d6e-b951fac36ec0");
//        private readonly Guid _AuctionManager1Id = new Guid("9c2d3cfa-a0fb-4cd0-8d6e-b951fac36ec5");
//        private readonly Guid _DeptManagerId = new Guid("9c2d3cfa-a0fb-4cd0-8d6e-b951fac36ec7");
//        private readonly Guid _CommitmentManagerId = new Guid("9c2d3cfa-a0fb-4cd0-8d6e-b951fac36ec9");
//        public void Configure(EntityTypeBuilder<ApplicationUserRoles> builder)
//        {
//            ApplicationUserRoles userRole = new ApplicationUserRoles
//            {
//                Id = new Guid("533810E7-AD5A-4883-AC15-A004A48D51D5"),
//                RoleId = RoleHelper.Admin,
//                UserId = _adminId
//            };
//        }
//    }
//}