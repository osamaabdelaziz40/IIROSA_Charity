using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Charities_Cities_CityId",
                table: "Charities");

            migrationBuilder.RenameColumn(
                name: "PhoneCode",
                table: "Countries",
                newName: "DialingCode");

            migrationBuilder.AddColumn<Guid>(
                name: "CharityId",
                table: "Sponsors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Sponsors",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_CharityId",
                table: "Sponsors",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Sponsors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "PeriodicOrphanReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PeriodicOrphanReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Orphans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_CharityId",
                table: "Orphans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Orphans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Families",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_CharityId",
                table: "Families",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Families",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Families",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FK_UserId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Departments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Countries",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlagIcon",
                table: "Countries",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Countries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Cities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Charities",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Charities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankAccount",
                table: "Charities",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BankId",
                table: "Charities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BossJobName",
                table: "Charities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BossName",
                table: "Charities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BossPhone1",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BossPhone2",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CenterId",
                table: "Charities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Charities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Charities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HomePhone",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Charities",
                type: "nvarchar(34)",
                maxLength: 34,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAddEnabled",
                table: "Charities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Charities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Charities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUpdateEnabled",
                table: "Charities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MailBox",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NGOType",
                table: "Charities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NgoMapLocation",
                table: "Charities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeIcon",
                table: "Charities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Phone2",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceivingDonations",
                table: "Charities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RegionId",
                table: "Charities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleJobName",
                table: "Charities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePhone1",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePhone2",
                table: "Charities",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StreetName",
                table: "Charities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Charities",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Charities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ApplicationRole",
                columns: table => new
                {
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DisplayNameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationRole", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUser",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserTypeId = table.Column<int>(type: "int", nullable: true),
                    Otp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredNotificationLanguage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AgancyId = table.Column<int>(type: "int", nullable: true),
                    CurrentToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwiftCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionTimeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TimeTypeCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TypeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionTimeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MissionType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MissionType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NGOType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NGOType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OfficeProjectTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypeDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeProjectTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Region",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegionCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Region", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Region_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketPriorities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SeverityLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ResponseTimeHours = table.Column<int>(type: "int", nullable: false, defaultValue: 24),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketPriorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportTicketStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ColorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsTerminalStatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTicketStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserRoles",
                columns: table => new
                {
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserRoles_ApplicationRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "ApplicationRole",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApplicationUserRoles_ApplicationUser_UserId",
                        column: x => x.UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Center",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CenterCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Center", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Center_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Center_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Region",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SupportTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    PriorityId = table.Column<int>(type: "int", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    IsSolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolutionDescription = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResolvedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    BrowserInfo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PageUrl = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    UserAction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssignedTo = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AttachmentFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttachmentFileSize = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportTicketCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "SupportTicketCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportTicketPriorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "SupportTicketPriorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SupportTickets_SupportTicketStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "SupportTicketStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Missions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MissionTarget = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MissionDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Details = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FK_MissionTypeId = table.Column<int>(type: "int", nullable: true),
                    FK_MissionTimeTypeId = table.Column<int>(type: "int", nullable: true),
                    MissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MissionCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsMissionCompleted = table.Column<bool>(type: "bit", nullable: false),
                    MissionCompletedTxt = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FK_CountryId = table.Column<int>(type: "int", nullable: true),
                    FK_RegionId = table.Column<int>(type: "int", nullable: true),
                    FK_CenterId = table.Column<int>(type: "int", nullable: true),
                    MissionLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Village = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FK_UserId = table.Column<Guid>(type: "uniqueidentifier", maxLength: 450, nullable: true),
                    EntityName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ConferenceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Missions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Missions_ApplicationUser_FK_UserId",
                        column: x => x.FK_UserId,
                        principalTable: "ApplicationUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Center_FK_CenterId",
                        column: x => x.FK_CenterId,
                        principalTable: "Center",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Countries_FK_CountryId",
                        column: x => x.FK_CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_MissionTimeTypes_FK_MissionTimeTypeId",
                        column: x => x.FK_MissionTimeTypeId,
                        principalTable: "MissionTimeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_MissionType_FK_MissionTypeId",
                        column: x => x.FK_MissionTypeId,
                        principalTable: "MissionType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Missions_Region_FK_RegionId",
                        column: x => x.FK_RegionId,
                        principalTable: "Region",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfficeProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ProjectHint = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProjectDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProjectEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FK_OfficeProjectTypeId = table.Column<int>(type: "int", nullable: true),
                    FK_CountryId = table.Column<int>(type: "int", nullable: true),
                    FK_RegionId = table.Column<int>(type: "int", nullable: true),
                    FK_CenterId = table.Column<int>(type: "int", nullable: true),
                    VillageName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProjectCostEGP = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProjectCostSAR = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DonorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BeneficiariesCount = table.Column<int>(type: "int", nullable: true),
                    BeneficiariesType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FK_CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FK_AttachedFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FK_ProjectReportFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsFinished = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfficeProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OfficeProjects_Center_FK_CenterId",
                        column: x => x.FK_CenterId,
                        principalTable: "Center",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeProjects_Charities_FK_CharityId",
                        column: x => x.FK_CharityId,
                        principalTable: "Charities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeProjects_Countries_FK_CountryId",
                        column: x => x.FK_CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeProjects_OfficeProjectTypes_FK_OfficeProjectTypeId",
                        column: x => x.FK_OfficeProjectTypeId,
                        principalTable: "OfficeProjectTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfficeProjects_Region_FK_RegionId",
                        column: x => x.FK_RegionId,
                        principalTable: "Region",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SeasonalAidCampaign",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CampaignType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BudgetCurrency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PerFamilyAllocation = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: true),
                    RegionId = table.Column<int>(type: "int", nullable: true),
                    CenterId = table.Column<int>(type: "int", nullable: true),
                    CharityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MaximumFamilies = table.Column<int>(type: "int", nullable: true),
                    FamilyType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MinChildrenAge = table.Column<int>(type: "int", nullable: true),
                    MaxChildrenAge = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosureNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalAidCampaign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonalAidCampaign_Center_CenterId",
                        column: x => x.CenterId,
                        principalTable: "Center",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeasonalAidCampaign_Charities_CharityId",
                        column: x => x.CharityId,
                        principalTable: "Charities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeasonalAidCampaign_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeasonalAidCampaign_Region_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Region",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TicketResponses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponseText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    IsInternalNote = table.Column<bool>(type: "bit", nullable: false),
                    RespondedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ResponderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ResponderEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AttachmentFilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AttachmentFileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AttachmentFileSize = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketResponses_SupportTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "SupportTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonalAidBeneficiary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocationAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRegistered = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistrationNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDistributed = table.Column<bool>(type: "bit", nullable: false),
                    DistributionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalAidBeneficiary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonalAidBeneficiary_Families_FamilyId",
                        column: x => x.FamilyId,
                        principalTable: "Families",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SeasonalAidBeneficiary_SeasonalAidCampaign_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "SeasonalAidCampaign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeasonalAidDistribution",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BeneficiaryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsDistributed = table.Column<bool>(type: "bit", nullable: false),
                    DistributionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AmountDistributed = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReceivedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientRelationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SignatureImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AttachmentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistributionMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistributorName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DistributorRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeasonalAidDistribution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeasonalAidDistribution_SeasonalAidBeneficiary_BeneficiaryId",
                        column: x => x.BeneficiaryId,
                        principalTable: "SeasonalAidBeneficiary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sponsors_CharityId",
                table: "Sponsors",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_BankId",
                table: "Charities",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_CenterId",
                table: "Charities",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_Email",
                table: "Charities",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_IsActive",
                table: "Charities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_IsLocked",
                table: "Charities",
                column: "IsLocked");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_RegionId",
                table: "Charities",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Charities_UserId",
                table: "Charities",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserRoles_RoleId",
                table: "ApplicationUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserRoles_UserId",
                table: "ApplicationUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Center_CountryId",
                table: "Center",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Center_RegionId",
                table: "Center",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_CenterId",
                table: "Missions",
                column: "FK_CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_CountryId",
                table: "Missions",
                column: "FK_CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_MissionTimeTypeId",
                table: "Missions",
                column: "FK_MissionTimeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_MissionTypeId",
                table: "Missions",
                column: "FK_MissionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_RegionId",
                table: "Missions",
                column: "FK_RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_UserId",
                table: "Missions",
                column: "FK_UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_FK_UserId_IsMissionCompleted",
                table: "Missions",
                columns: new[] { "FK_UserId", "IsMissionCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Missions_IsMissionCompleted",
                table: "Missions",
                column: "IsMissionCompleted");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MissionCompletedDate",
                table: "Missions",
                column: "MissionCompletedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MissionDate",
                table: "Missions",
                column: "MissionDate");

            migrationBuilder.CreateIndex(
                name: "IX_Missions_MissionDate_IsMissionCompleted",
                table: "Missions",
                columns: new[] { "MissionDate", "IsMissionCompleted" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_AttachedFileId",
                table: "OfficeProjects",
                column: "FK_AttachedFileId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_CenterId",
                table: "OfficeProjects",
                column: "FK_CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_CharityId",
                table: "OfficeProjects",
                column: "FK_CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_CharityId_IsFinished",
                table: "OfficeProjects",
                columns: new[] { "FK_CharityId", "IsFinished" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_CountryId",
                table: "OfficeProjects",
                column: "FK_CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_CountryId_FK_RegionId_FK_CenterId",
                table: "OfficeProjects",
                columns: new[] { "FK_CountryId", "FK_RegionId", "FK_CenterId" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_OfficeProjectTypeId",
                table: "OfficeProjects",
                column: "FK_OfficeProjectTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_ProjectReportFileId",
                table: "OfficeProjects",
                column: "FK_ProjectReportFileId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_FK_RegionId",
                table: "OfficeProjects",
                column: "FK_RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_IsFinished",
                table: "OfficeProjects",
                column: "IsFinished");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_ProjectDate",
                table: "OfficeProjects",
                column: "ProjectDate");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_ProjectDate_IsFinished",
                table: "OfficeProjects",
                columns: new[] { "ProjectDate", "IsFinished" });

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_ProjectEndDate",
                table: "OfficeProjects",
                column: "ProjectEndDate");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjects_ProjectName",
                table: "OfficeProjects",
                column: "ProjectName");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjectTypes_IsActive",
                table: "OfficeProjectTypes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeProjectTypes_TypeCode",
                table: "OfficeProjectTypes",
                column: "TypeCode",
                unique: true,
                filter: "[TypeCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Region_CountryId",
                table: "Region",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidBeneficiary_CampaignId",
                table: "SeasonalAidBeneficiary",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidBeneficiary_FamilyId",
                table: "SeasonalAidBeneficiary",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidCampaign_CenterId",
                table: "SeasonalAidCampaign",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidCampaign_CharityId",
                table: "SeasonalAidCampaign",
                column: "CharityId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidCampaign_CountryId",
                table: "SeasonalAidCampaign",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidCampaign_RegionId",
                table: "SeasonalAidCampaign",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_SeasonalAidDistribution_BeneficiaryId",
                table: "SeasonalAidDistribution",
                column: "BeneficiaryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketCategories_IsActive",
                table: "SupportTicketCategories",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketCategories_SortOrder",
                table: "SupportTicketCategories",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketPriorities_IsActive",
                table: "SupportTicketPriorities",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketPriorities_SeverityLevel",
                table: "SupportTicketPriorities",
                column: "SeverityLevel");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_AssignedTo",
                table: "SupportTickets",
                column: "AssignedTo");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CategoryId",
                table: "SupportTickets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CreatedByUserId",
                table: "SupportTickets",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_CreatedOn",
                table: "SupportTickets",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_IsSolved",
                table: "SupportTickets",
                column: "IsSolved");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_PriorityId",
                table: "SupportTickets",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_ResolvedOn",
                table: "SupportTickets",
                column: "ResolvedOn");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_StatusId",
                table: "SupportTickets",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketStatuses_IsActive",
                table: "SupportTicketStatuses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketStatuses_IsTerminalStatus",
                table: "SupportTicketStatuses",
                column: "IsTerminalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTicketStatuses_SortOrder",
                table: "SupportTicketStatuses",
                column: "SortOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_CreatedOn",
                table: "TicketResponses",
                column: "CreatedOn");

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_IsInternalNote",
                table: "TicketResponses",
                column: "IsInternalNote");

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_RespondedByUserId",
                table: "TicketResponses",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketResponses_TicketId",
                table: "TicketResponses",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Charities_Bank_BankId",
                table: "Charities",
                column: "BankId",
                principalTable: "Bank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Charities_Center_CenterId",
                table: "Charities",
                column: "CenterId",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Charities_Cities_CityId",
                table: "Charities",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Charities_Region_RegionId",
                table: "Charities",
                column: "RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsors_Charities_CharityId",
                table: "Sponsors",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Charities_Bank_BankId",
                table: "Charities");

            migrationBuilder.DropForeignKey(
                name: "FK_Charities_Center_CenterId",
                table: "Charities");

            migrationBuilder.DropForeignKey(
                name: "FK_Charities_Cities_CityId",
                table: "Charities");

            migrationBuilder.DropForeignKey(
                name: "FK_Charities_Region_RegionId",
                table: "Charities");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsors_Charities_CharityId",
                table: "Sponsors");

            migrationBuilder.DropTable(
                name: "ApplicationUserRoles");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "Missions");

            migrationBuilder.DropTable(
                name: "NGOType");

            migrationBuilder.DropTable(
                name: "OfficeProjects");

            migrationBuilder.DropTable(
                name: "ProjectType");

            migrationBuilder.DropTable(
                name: "SeasonalAidDistribution");

            migrationBuilder.DropTable(
                name: "TicketResponses");

            migrationBuilder.DropTable(
                name: "ApplicationRole");

            migrationBuilder.DropTable(
                name: "ApplicationUser");

            migrationBuilder.DropTable(
                name: "MissionTimeTypes");

            migrationBuilder.DropTable(
                name: "MissionType");

            migrationBuilder.DropTable(
                name: "OfficeProjectTypes");

            migrationBuilder.DropTable(
                name: "SeasonalAidBeneficiary");

            migrationBuilder.DropTable(
                name: "SupportTickets");

            migrationBuilder.DropTable(
                name: "SeasonalAidCampaign");

            migrationBuilder.DropTable(
                name: "SupportTicketCategories");

            migrationBuilder.DropTable(
                name: "SupportTicketPriorities");

            migrationBuilder.DropTable(
                name: "SupportTicketStatuses");

            migrationBuilder.DropTable(
                name: "Center");

            migrationBuilder.DropTable(
                name: "Region");

            migrationBuilder.DropIndex(
                name: "IX_Sponsors_CharityId",
                table: "Sponsors");

            migrationBuilder.DropIndex(
                name: "IX_Charities_BankId",
                table: "Charities");

            migrationBuilder.DropIndex(
                name: "IX_Charities_CenterId",
                table: "Charities");

            migrationBuilder.DropIndex(
                name: "IX_Charities_Email",
                table: "Charities");

            migrationBuilder.DropIndex(
                name: "IX_Charities_IsActive",
                table: "Charities");

            migrationBuilder.DropIndex(
                name: "IX_Charities_IsLocked",
                table: "Charities");

            migrationBuilder.DropIndex(
                name: "IX_Charities_RegionId",
                table: "Charities");

            migrationBuilder.DropIndex(
                name: "IX_Charities_UserId",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "CharityId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "FK_CharityId",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Sponsors");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "PeriodicOrphanReports");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PeriodicOrphanReports");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "FK_CharityId",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Orphans");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "FK_CharityId",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Families");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "FK_UserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "FlagIcon",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Countries");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "BankAccount",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "BankId",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "BossJobName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "BossName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "BossPhone1",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "BossPhone2",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "CenterId",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "HomePhone",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "IsAddEnabled",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "IsUpdateEnabled",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "MailBox",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "NGOType",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "NgoMapLocation",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "OfficeIcon",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "Phone2",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "ReceivingDonations",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "RegionId",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "ResponsibleJobName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "ResponsiblePhone1",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "ResponsiblePhone2",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "StreetName",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Charities");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "Charities");

            migrationBuilder.RenameColumn(
                name: "DialingCode",
                table: "Countries",
                newName: "PhoneCode");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Charities",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Charities",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_Charities_Cities_CityId",
                table: "Charities",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
