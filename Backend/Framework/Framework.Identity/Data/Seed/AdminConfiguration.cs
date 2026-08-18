//using Framework.Identity.Data.Entities;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Metadata.Builders;
//using System;
//using System.Collections.Generic;

//namespace Framework.Identity.Data.Seed
//{
//    public class AdminConfiguration : IEntityTypeConfiguration<ApplicationUser>
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
//        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
//        {
//            List<ApplicationUser> Data = new List<ApplicationUser>();
//            var admin = new ApplicationUser
//            {
//                Id = _adminId,
//                UserName = "admin",
//                NormalizedUserName = "ADMIN",
//                FullName = "Administrator",
//                NormalizedEmail = "AELREDINY@SURE.COM.SA",
//                PhoneNumber = "+201001726484",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "aelrediny@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            admin.PasswordHash = PassGenerate(admin);
//            Data.Add(admin);

//            var individual = new ApplicationUser
//            {
//                Id = _individualId,
//                UserName = "individual",
//                NormalizedUserName = "INDIVIDUAL",
//                FullName = "Individual",
//                NormalizedEmail = "Individual@SURE.COM.SA",
//                PhoneNumber = "+201000000000",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "Individual@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            individual.PasswordHash = PassGenerate(individual);
//            Data.Add(individual);

//            var company = new ApplicationUser
//            {
//                Id = _companyId,
//                UserName = "company",
//                NormalizedUserName = "COMPANY",
//                FullName = "Company",
//                NormalizedEmail = "Company@SURE.COM.SA",
//                PhoneNumber = "+201000000111",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "Company@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            company.PasswordHash = PassGenerate(company);
//            Data.Add(company);

//            var AuctionSupervisor = new ApplicationUser
//            {
//                Id = _AuctionSupervisorId,
//                UserName = "AuctionSupervisor",
//                NormalizedUserName = "AuctionSupervisor",
//                FullName = "AuctionSupervisor",
//                NormalizedEmail = "auctionSupervisor@SURE.COM.SA",
//                PhoneNumber = "+201000001111",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "auctionSupervisor@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            AuctionSupervisor.PasswordHash = PassGenerate(AuctionSupervisor);
//            Data.Add(AuctionSupervisor);

//            var CentralOperationsOfficer = new ApplicationUser
//            {
//                Id = _centralOperationsOfficerId,
//                UserName = "centralOperationsOfficer",
//                NormalizedUserName = "centralOperationsOfficer",
//                FullName = "centralOperationsOfficer",
//                NormalizedEmail = "centralOperationsOfficer@SURE.COM.SA",
//                PhoneNumber = "+201000002222",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "centralOperationsOfficer@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            CentralOperationsOfficer.PasswordHash = PassGenerate(CentralOperationsOfficer);
//            Data.Add(CentralOperationsOfficer);
//            var HeadOfSector = new ApplicationUser
//            {
//                Id = _headOfSectorId,
//                UserName = "HeadOfSector",
//                NormalizedUserName = "HeadOfSector",
//                FullName = "HeadOfSector",
//                NormalizedEmail = "HeadOfSector@SURE.COM.SA",
//                PhoneNumber = "+201000001111",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "HeadOfSector@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            HeadOfSector.PasswordHash = PassGenerate(HeadOfSector);
//            Data.Add(HeadOfSector);

//            var SectorHead1 = new ApplicationUser
//            {
//                Id = _SectorHead1Id,
//                UserName = "SectorHead1",
//                NormalizedUserName = "SectorHead1",
//                FullName = "رئيس القطاع 1",
//                NormalizedEmail = "SectorHead1@SURE.COM.SA",
//                PhoneNumber = "+201000000000",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "SectorHead1@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            SectorHead1.PasswordHash = PassGenerate(SectorHead1);
//            Data.Add(SectorHead1);
//            var CentralOperationGM = new ApplicationUser
//            {
//                Id = _CentralOperationGMId,
//                UserName = "CentralOperationGM",
//                NormalizedUserName = "CentralOperationGM",
//                FullName = "مدير عام العمليات المركزية",
//                NormalizedEmail = "CentralOperationGM@SURE.COM.SA",
//                PhoneNumber = "+201000000000",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "CentralOperationGM@SURE.COM.SA",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            CentralOperationGM.PasswordHash = PassGenerate(CentralOperationGM);
//            Data.Add(CentralOperationGM);
//            var AuctionManager1 = new ApplicationUser
//            {
//                Id = _AuctionManager1Id,
//                UserName = "AuctionManager1",
//                NormalizedUserName = "AuctionManager1",
//                FullName = "مدير المزادات 1",
//                NormalizedEmail = "AuctionManager1@SURE.COM.SA",
//                PhoneNumber = "+201000000000",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "AuctionManager1@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            AuctionManager1.PasswordHash = PassGenerate(AuctionManager1);
//            Data.Add(AuctionManager1);
//            var DeptManager = new ApplicationUser
//            {
//                Id = _DeptManagerId,
//                UserName = "DeptManager",
//                NormalizedUserName = "DeptManager",
//                FullName = "مدير إدارة الدين",
//                NormalizedEmail = "DeptManager@SURE.COM.SA",
//                PhoneNumber = "+201000000000",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "DeptManager@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            DeptManager.PasswordHash = PassGenerate(DeptManager);
//            Data.Add(DeptManager);
//            var CommitmentManager = new ApplicationUser
//            {
//                Id = _CommitmentManagerId,
//                UserName = "CommitmentManager",
//                NormalizedUserName = "CommitmentManager",
//                FullName = "مدير الالتزام",
//                NormalizedEmail = "CommitmentManager@SURE.COM.SA",
//                PhoneNumber = "+201000000000",
//                EmailConfirmed = true,
//                PhoneNumberConfirmed = true,
//                SecurityStamp = new Guid().ToString("D"),
//                Email = "CommitmentManager@sure.com.sa",
//                IsExternalUser = false,
//                IsActive = true,
//                CreatedBy = "Admin",
//                CreatedOn = DateTime.Now
//            };
//            CommitmentManager.PasswordHash = PassGenerate(CommitmentManager);
//            Data.Add(CommitmentManager);

//            builder.HasData(Data);
//        }

//        public string PassGenerate(ApplicationUser user)
//        {
//            var passHash = new PasswordHasher<ApplicationUser>();
//            return passHash.HashPassword(user, "P@ssw0rd");

//        }
//    }
//}