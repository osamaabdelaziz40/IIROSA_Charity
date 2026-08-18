using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIROSA.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaConventions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Center_Countries_CountryId",
                table: "Center");

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
                name: "FK_Charities_Countries_CountryId",
                table: "Charities");

            migrationBuilder.DropForeignKey(
                name: "FK_Charities_Region_RegionId",
                table: "Charities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_CountryId1",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_Charities_CharityId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_Cities_CityId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_Countries_CountryId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_HousingType_HousingTypeId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Families_LivingCondition_LivingConditionId",
                table: "Families");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_Families_FamilyId",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Charities_CharityId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Countries_CountryId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Families_FamilyId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_Departments_DepartmentId",
                table: "Incoming");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_ApplicationUser_FK_UserId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Center_FK_CenterId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Countries_FK_CountryId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MissionTimeTypes_FK_MissionTimeTypeId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_MissionType_FK_MissionTypeId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Missions_Region_FK_RegionId",
                table: "Missions");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_Families_FamilyId",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProjects_Center_FK_CenterId",
                table: "OfficeProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProjects_Charities_FK_CharityId",
                table: "OfficeProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProjects_Countries_FK_CountryId",
                table: "OfficeProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProjects_OfficeProjectTypes_FK_OfficeProjectTypeId",
                table: "OfficeProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProjects_Region_FK_RegionId",
                table: "OfficeProjects");

            migrationBuilder.DropForeignKey(
                name: "FK_OrphanPaymentItems_OrphanPayments_OrphanPaymentId",
                table: "OrphanPaymentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrphanPaymentItems_Orphans_OrphanId",
                table: "OrphanPaymentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_Charities_CharityId",
                table: "Orphans");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_EducationLevel_EducationLevelId",
                table: "Orphans");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_Families_FamilyId",
                table: "Orphans");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_HealthStatus_HealthStatusId",
                table: "Orphans");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphans_Sponsors_SponsorId",
                table: "Orphans");

            migrationBuilder.DropForeignKey(
                name: "FK_Outgoing_Departments_DepartmentId",
                table: "Outgoing");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReports_Charities_CharityId",
                table: "PeriodicOrphanReports");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReports_Orphans_OrphanId",
                table: "PeriodicOrphanReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_Families_FamilyId",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Region_Countries_CountryId",
                table: "Region");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalAidBeneficiary_Families_FamilyId",
                table: "SeasonalAidBeneficiary");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalAidCampaign_Charities_CharityId",
                table: "SeasonalAidCampaign");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalAidCampaign_Countries_CountryId",
                table: "SeasonalAidCampaign");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsors_Charities_CharityId",
                table: "Sponsors");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsors_Cities_CityId",
                table: "Sponsors");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsors_Countries_CountryId",
                table: "Sponsors");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_SupportTicketCategories_CategoryId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_SupportTicketPriorities_PriorityId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_SupportTicketStatuses_StatusId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketResponses_SupportTickets_TicketId",
                table: "TicketResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketResponses",
                table: "TicketResponses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicketStatuses",
                table: "SupportTicketStatuses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTickets",
                table: "SupportTickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicketPriorities",
                table: "SupportTicketPriorities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicketCategories",
                table: "SupportTicketCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sponsors",
                table: "Sponsors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodicOrphanReports",
                table: "PeriodicOrphanReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orphans",
                table: "Orphans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrphanPayments",
                table: "OrphanPayments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrphanPaymentItems",
                table: "OrphanPaymentItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeProjectTypes",
                table: "OfficeProjectTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeProjects",
                table: "OfficeProjects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissionTimeTypes",
                table: "MissionTimeTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Missions",
                table: "Missions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Families",
                table: "Families");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Departments",
                table: "Departments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Countries",
                table: "Countries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cities",
                table: "Cities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Charities",
                table: "Charities");

            migrationBuilder.EnsureSchema(
                name: "IIROSA");

            migrationBuilder.EnsureSchema(
                name: "Lookup");

            migrationBuilder.RenameTable(
                name: "Outgoing",
                newName: "Outgoing",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Incoming",
                newName: "Incoming",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "TicketResponses",
                newName: "TicketResponse",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "SupportTicketStatuses",
                newName: "SupportTicketStatus",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "SupportTickets",
                newName: "SupportTicket",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "SupportTicketPriorities",
                newName: "SupportTicketPriority",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "SupportTicketCategories",
                newName: "SupportTicketCategory",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Sponsors",
                newName: "Sponsor",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "PeriodicOrphanReports",
                newName: "PeriodicOrphanReport",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Orphans",
                newName: "Orphan",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "OrphanPayments",
                newName: "OrphanPayment",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "OrphanPaymentItems",
                newName: "OrphanPaymentItem",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "OfficeProjectTypes",
                newName: "OfficeProjectType",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "OfficeProjects",
                newName: "OfficeProject",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "MissionTimeTypes",
                newName: "MissionTimeType",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Missions",
                newName: "Mission",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Families",
                newName: "Family",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "Employee",
                newSchema: "IIROSA");

            migrationBuilder.RenameTable(
                name: "Departments",
                newName: "Department",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Countries",
                newName: "Country",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Cities",
                newName: "City",
                newSchema: "Lookup");

            migrationBuilder.RenameTable(
                name: "Charities",
                newName: "Charity",
                newSchema: "IIROSA");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponses_TicketId",
                schema: "IIROSA",
                table: "TicketResponse",
                newName: "IX_TicketResponse_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponses_RespondedByUserId",
                schema: "IIROSA",
                table: "TicketResponse",
                newName: "IX_TicketResponse_RespondedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponses_IsInternalNote",
                schema: "IIROSA",
                table: "TicketResponse",
                newName: "IX_TicketResponse_IsInternalNote");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponses_CreatedOn",
                schema: "IIROSA",
                table: "TicketResponse",
                newName: "IX_TicketResponse_CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketStatuses_SortOrder",
                schema: "Lookup",
                table: "SupportTicketStatus",
                newName: "IX_SupportTicketStatus_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketStatuses_IsTerminalStatus",
                schema: "Lookup",
                table: "SupportTicketStatus",
                newName: "IX_SupportTicketStatus_IsTerminalStatus");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketStatuses_IsActive",
                schema: "Lookup",
                table: "SupportTicketStatus",
                newName: "IX_SupportTicketStatus_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_StatusId",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_ResolvedOn",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_ResolvedOn");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_PriorityId",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_PriorityId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_IsSolved",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_IsSolved");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_CreatedOn",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_CreatedByUserId",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_CategoryId",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTickets_AssignedTo",
                schema: "IIROSA",
                table: "SupportTicket",
                newName: "IX_SupportTicket_AssignedTo");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketPriorities_SeverityLevel",
                schema: "Lookup",
                table: "SupportTicketPriority",
                newName: "IX_SupportTicketPriority_SeverityLevel");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketPriorities_IsActive",
                schema: "Lookup",
                table: "SupportTicketPriority",
                newName: "IX_SupportTicketPriority_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketCategories_SortOrder",
                schema: "Lookup",
                table: "SupportTicketCategory",
                newName: "IX_SupportTicketCategory_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketCategories_IsActive",
                schema: "Lookup",
                table: "SupportTicketCategory",
                newName: "IX_SupportTicketCategory_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsors_Email",
                schema: "IIROSA",
                table: "Sponsor",
                newName: "IX_Sponsor_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsors_CountryId",
                schema: "IIROSA",
                table: "Sponsor",
                newName: "IX_Sponsor_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsors_Code",
                schema: "IIROSA",
                table: "Sponsor",
                newName: "IX_Sponsor_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsors_CityId",
                schema: "IIROSA",
                table: "Sponsor",
                newName: "IX_Sponsor_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsors_CharityId",
                schema: "IIROSA",
                table: "Sponsor",
                newName: "IX_Sponsor_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReports_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                newName: "IX_PeriodicOrphanReport_ReportYear");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReports_ReportMonth",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                newName: "IX_PeriodicOrphanReport_ReportMonth");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReports_OrphanId_ReportMonth_ReportYear",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                newName: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReports_OrphanId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                newName: "IX_PeriodicOrphanReport_OrphanId");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReports_CharityId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                newName: "IX_PeriodicOrphanReport_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphans_SponsorId",
                schema: "IIROSA",
                table: "Orphan",
                newName: "IX_Orphan_SponsorId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphans_HealthStatusId",
                schema: "IIROSA",
                table: "Orphan",
                newName: "IX_Orphan_HealthStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphans_FamilyId",
                schema: "IIROSA",
                table: "Orphan",
                newName: "IX_Orphan_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphans_EducationLevelId",
                schema: "IIROSA",
                table: "Orphan",
                newName: "IX_Orphan_EducationLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphans_Code",
                schema: "IIROSA",
                table: "Orphan",
                newName: "IX_Orphan_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Orphans_CharityId",
                schema: "IIROSA",
                table: "Orphan",
                newName: "IX_Orphan_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_ShowOrder",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_ShowOrder");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_PaymentPeriodTo",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_PaymentPeriodTo");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_PaymentPeriodFrom",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_PaymentPeriodFrom");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_IsBatchUploaded",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_IsBatchUploaded");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_GroupName",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_GroupName");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_GroupDate",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_GroupDate");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayments_BatchNo",
                schema: "IIROSA",
                table: "OrphanPayment",
                newName: "IX_OrphanPayment_BatchNo");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPaymentItems_OrphanPaymentId_OrphanId",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                newName: "IX_OrphanPaymentItem_OrphanPaymentId_OrphanId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPaymentItems_OrphanPaymentId",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                newName: "IX_OrphanPaymentItem_OrphanPaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPaymentItems_OrphanId",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                newName: "IX_OrphanPaymentItem_OrphanId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjectTypes_TypeCode",
                schema: "Lookup",
                table: "OfficeProjectType",
                newName: "IX_OfficeProjectType_TypeCode");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjectTypes_IsActive",
                schema: "Lookup",
                table: "OfficeProjectType",
                newName: "IX_OfficeProjectType_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_ProjectName",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_ProjectName");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_ProjectEndDate",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_ProjectEndDate");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_ProjectDate_IsFinished",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_ProjectDate_IsFinished");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_ProjectDate",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_ProjectDate");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_IsFinished",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_IsFinished");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_RegionId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_ProjectReportFileId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_ProjectReportFileId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_OfficeProjectTypeId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_OfficeProjectTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_CountryId_FK_RegionId_FK_CenterId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_CountryId_FK_RegionId_FK_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_CountryId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_CharityId_IsFinished",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_CharityId_IsFinished");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_CharityId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_CenterId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjects_FK_AttachedFileId",
                schema: "IIROSA",
                table: "OfficeProject",
                newName: "IX_OfficeProject_FK_AttachedFileId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_MissionDate_IsMissionCompleted",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_MissionDate_IsMissionCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_MissionDate",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_MissionDate");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_MissionCompletedDate",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_MissionCompletedDate");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_IsMissionCompleted",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_IsMissionCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_UserId_IsMissionCompleted",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_UserId_IsMissionCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_UserId",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_RegionId",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_MissionTypeId",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_MissionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_MissionTimeTypeId",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_MissionTimeTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_CountryId",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Missions_FK_CenterId",
                schema: "IIROSA",
                table: "Mission",
                newName: "IX_Mission_FK_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_LivingConditionId",
                schema: "IIROSA",
                table: "Family",
                newName: "IX_Family_LivingConditionId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_HousingTypeId",
                schema: "IIROSA",
                table: "Family",
                newName: "IX_Family_HousingTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_CountryId",
                schema: "IIROSA",
                table: "Family",
                newName: "IX_Family_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_Code",
                schema: "IIROSA",
                table: "Family",
                newName: "IX_Family_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Families_CityId",
                schema: "IIROSA",
                table: "Family",
                newName: "IX_Family_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Families_CharityId",
                schema: "IIROSA",
                table: "Family",
                newName: "IX_Family_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_NationalId",
                schema: "IIROSA",
                table: "Employee",
                newName: "IX_Employee_NationalId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_Email",
                schema: "IIROSA",
                table: "Employee",
                newName: "IX_Employee_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_DepartmentId",
                schema: "IIROSA",
                table: "Employee",
                newName: "IX_Employee_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_Code",
                schema: "IIROSA",
                table: "Employee",
                newName: "IX_Employee_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_CountryId1",
                schema: "Lookup",
                table: "City",
                newName: "IX_City_CountryId1");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_CountryId",
                schema: "Lookup",
                table: "City",
                newName: "IX_City_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_UserId",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_RegionId",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_Name",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_IsLocked",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_IsLocked");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_IsActive",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_Email",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_CountryId",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_Code",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_CityId",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_CenterId",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Charities_BankId",
                schema: "IIROSA",
                table: "Charity",
                newName: "IX_Charity_BankId");

            migrationBuilder.AddColumn<int>(
                name: "RelativesCount",
                schema: "IIROSA",
                table: "Family",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketResponse",
                schema: "IIROSA",
                table: "TicketResponse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicketStatus",
                schema: "Lookup",
                table: "SupportTicketStatus",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicket",
                schema: "IIROSA",
                table: "SupportTicket",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicketPriority",
                schema: "Lookup",
                table: "SupportTicketPriority",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicketCategory",
                schema: "Lookup",
                table: "SupportTicketCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sponsor",
                schema: "IIROSA",
                table: "Sponsor",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodicOrphanReport",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orphan",
                schema: "IIROSA",
                table: "Orphan",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrphanPayment",
                schema: "IIROSA",
                table: "OrphanPayment",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrphanPaymentItem",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeProjectType",
                schema: "Lookup",
                table: "OfficeProjectType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeProject",
                schema: "IIROSA",
                table: "OfficeProject",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissionTimeType",
                schema: "Lookup",
                table: "MissionTimeType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Mission",
                schema: "IIROSA",
                table: "Mission",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Family",
                schema: "IIROSA",
                table: "Family",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employee",
                schema: "IIROSA",
                table: "Employee",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Department",
                schema: "Lookup",
                table: "Department",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Country",
                schema: "Lookup",
                table: "Country",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_City",
                schema: "Lookup",
                table: "City",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Charity",
                schema: "IIROSA",
                table: "Charity",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Relative",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FamilyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RelationshipType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationLevelId = table.Column<int>(type: "int", nullable: true),
                    Job = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HealthStatusId = table.Column<int>(type: "int", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsAlive = table.Column<bool>(type: "bit", nullable: false),
                    IsLivingWithFamily = table.Column<bool>(type: "bit", nullable: false),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relative", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Relative_EducationLevel_EducationLevelId",
                        column: x => x.EducationLevelId,
                        principalTable: "EducationLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Relative_Family_FamilyId",
                        column: x => x.FamilyId,
                        principalSchema: "IIROSA",
                        principalTable: "Family",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Relative_HealthStatus_HealthStatusId",
                        column: x => x.HealthStatusId,
                        principalTable: "HealthStatus",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Relative_EducationLevelId",
                table: "Relative",
                column: "EducationLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Relative_FamilyId",
                table: "Relative",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Relative_HealthStatusId",
                table: "Relative",
                column: "HealthStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Country_CountryId",
                table: "Center",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Charity_Bank_BankId",
                schema: "IIROSA",
                table: "Charity",
                column: "BankId",
                principalTable: "Bank",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Charity_Center_CenterId",
                schema: "IIROSA",
                table: "Charity",
                column: "CenterId",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Charity_City_CityId",
                schema: "IIROSA",
                table: "Charity",
                column: "CityId",
                principalSchema: "Lookup",
                principalTable: "City",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Charity_Country_CountryId",
                schema: "IIROSA",
                table: "Charity",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Charity_Region_RegionId",
                schema: "IIROSA",
                table: "Charity",
                column: "RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_City_Country_CountryId",
                schema: "Lookup",
                table: "City",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_City_Country_CountryId1",
                schema: "Lookup",
                table: "City",
                column: "CountryId1",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Department_DepartmentId",
                schema: "IIROSA",
                table: "Employee",
                column: "DepartmentId",
                principalSchema: "Lookup",
                principalTable: "Department",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_Charity_CharityId",
                schema: "IIROSA",
                table: "Family",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_City_CityId",
                schema: "IIROSA",
                table: "Family",
                column: "CityId",
                principalSchema: "Lookup",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_Country_CountryId",
                schema: "IIROSA",
                table: "Family",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Family_HousingType_HousingTypeId",
                schema: "IIROSA",
                table: "Family",
                column: "HousingTypeId",
                principalTable: "HousingType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Family_LivingCondition_LivingConditionId",
                schema: "IIROSA",
                table: "Family",
                column: "LivingConditionId",
                principalTable: "LivingCondition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_Family_FamilyId",
                table: "Father",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Charity_CharityId",
                table: "HousingProject",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Country_CountryId",
                table: "HousingProject",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Family_FamilyId",
                table: "HousingProject",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_Department_DepartmentId",
                schema: "IIROSA",
                table: "Incoming",
                column: "DepartmentId",
                principalSchema: "Lookup",
                principalTable: "Department",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_Center_FK_CenterId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_CenterId",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_Country_FK_CountryId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_MissionTimeType_FK_MissionTimeTypeId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_MissionTimeTypeId",
                principalSchema: "Lookup",
                principalTable: "MissionTimeType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_MissionType_FK_MissionTypeId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_MissionTypeId",
                principalTable: "MissionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mission_Region_FK_RegionId",
                schema: "IIROSA",
                table: "Mission",
                column: "FK_RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_Family_FamilyId",
                table: "Mother",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProject_Center_FK_CenterId",
                schema: "IIROSA",
                table: "OfficeProject",
                column: "FK_CenterId",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProject_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "OfficeProject",
                column: "FK_CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProject_Country_FK_CountryId",
                schema: "IIROSA",
                table: "OfficeProject",
                column: "FK_CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProject_OfficeProjectType_FK_OfficeProjectTypeId",
                schema: "IIROSA",
                table: "OfficeProject",
                column: "FK_OfficeProjectTypeId",
                principalSchema: "Lookup",
                principalTable: "OfficeProjectType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProject_Region_FK_RegionId",
                schema: "IIROSA",
                table: "OfficeProject",
                column: "FK_RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_Charity_CharityId",
                schema: "IIROSA",
                table: "Orphan",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Orphan",
                column: "EducationLevelId",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_Family_FamilyId",
                schema: "IIROSA",
                table: "Orphan",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Orphan",
                column: "HealthStatusId",
                principalTable: "HealthStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphan_Sponsor_SponsorId",
                schema: "IIROSA",
                table: "Orphan",
                column: "SponsorId",
                principalSchema: "IIROSA",
                principalTable: "Sponsor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrphanPaymentItem_OrphanPayment_OrphanPaymentId",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                column: "OrphanPaymentId",
                principalSchema: "IIROSA",
                principalTable: "OrphanPayment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrphanPaymentItem_Orphan_OrphanId",
                schema: "IIROSA",
                table: "OrphanPaymentItem",
                column: "OrphanId",
                principalSchema: "IIROSA",
                principalTable: "Orphan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Outgoing_Department_DepartmentId",
                schema: "IIROSA",
                table: "Outgoing",
                column: "DepartmentId",
                principalSchema: "Lookup",
                principalTable: "Department",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReport_Charity_CharityId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReport_Orphan_OrphanId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport",
                column: "OrphanId",
                principalSchema: "IIROSA",
                principalTable: "Orphan",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_Family_FamilyId",
                table: "Provider",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Region_Country_CountryId",
                table: "Region",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalAidBeneficiary_Family_FamilyId",
                table: "SeasonalAidBeneficiary",
                column: "FamilyId",
                principalSchema: "IIROSA",
                principalTable: "Family",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalAidCampaign_Charity_CharityId",
                table: "SeasonalAidCampaign",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalAidCampaign_Country_CountryId",
                table: "SeasonalAidCampaign",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsor_Charity_CharityId",
                schema: "IIROSA",
                table: "Sponsor",
                column: "CharityId",
                principalSchema: "IIROSA",
                principalTable: "Charity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsor_City_CityId",
                schema: "IIROSA",
                table: "Sponsor",
                column: "CityId",
                principalSchema: "Lookup",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsor_Country_CountryId",
                schema: "IIROSA",
                table: "Sponsor",
                column: "CountryId",
                principalSchema: "Lookup",
                principalTable: "Country",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_SupportTicketCategory_CategoryId",
                schema: "IIROSA",
                table: "SupportTicket",
                column: "CategoryId",
                principalSchema: "Lookup",
                principalTable: "SupportTicketCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_SupportTicketPriority_PriorityId",
                schema: "IIROSA",
                table: "SupportTicket",
                column: "PriorityId",
                principalSchema: "Lookup",
                principalTable: "SupportTicketPriority",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTicket_SupportTicketStatus_StatusId",
                schema: "IIROSA",
                table: "SupportTicket",
                column: "StatusId",
                principalSchema: "Lookup",
                principalTable: "SupportTicketStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketResponse_SupportTicket_TicketId",
                schema: "IIROSA",
                table: "TicketResponse",
                column: "TicketId",
                principalSchema: "IIROSA",
                principalTable: "SupportTicket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Center_Country_CountryId",
                table: "Center");

            migrationBuilder.DropForeignKey(
                name: "FK_Charity_Bank_BankId",
                schema: "IIROSA",
                table: "Charity");

            migrationBuilder.DropForeignKey(
                name: "FK_Charity_Center_CenterId",
                schema: "IIROSA",
                table: "Charity");

            migrationBuilder.DropForeignKey(
                name: "FK_Charity_City_CityId",
                schema: "IIROSA",
                table: "Charity");

            migrationBuilder.DropForeignKey(
                name: "FK_Charity_Country_CountryId",
                schema: "IIROSA",
                table: "Charity");

            migrationBuilder.DropForeignKey(
                name: "FK_Charity_Region_RegionId",
                schema: "IIROSA",
                table: "Charity");

            migrationBuilder.DropForeignKey(
                name: "FK_City_Country_CountryId",
                schema: "Lookup",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_City_Country_CountryId1",
                schema: "Lookup",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Department_DepartmentId",
                schema: "IIROSA",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_Charity_CharityId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_City_CityId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_Country_CountryId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_HousingType_HousingTypeId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Family_LivingCondition_LivingConditionId",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropForeignKey(
                name: "FK_Father_Family_FamilyId",
                table: "Father");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Charity_CharityId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Country_CountryId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_HousingProject_Family_FamilyId",
                table: "HousingProject");

            migrationBuilder.DropForeignKey(
                name: "FK_Incoming_Department_DepartmentId",
                schema: "IIROSA",
                table: "Incoming");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_ApplicationUser_FK_UserId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_Center_FK_CenterId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_Country_FK_CountryId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_MissionTimeType_FK_MissionTimeTypeId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_MissionType_FK_MissionTypeId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mission_Region_FK_RegionId",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropForeignKey(
                name: "FK_Mother_Family_FamilyId",
                table: "Mother");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProject_Center_FK_CenterId",
                schema: "IIROSA",
                table: "OfficeProject");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProject_Charity_FK_CharityId",
                schema: "IIROSA",
                table: "OfficeProject");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProject_Country_FK_CountryId",
                schema: "IIROSA",
                table: "OfficeProject");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProject_OfficeProjectType_FK_OfficeProjectTypeId",
                schema: "IIROSA",
                table: "OfficeProject");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeProject_Region_FK_RegionId",
                schema: "IIROSA",
                table: "OfficeProject");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_Charity_CharityId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_EducationLevel_EducationLevelId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_Family_FamilyId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_HealthStatus_HealthStatusId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropForeignKey(
                name: "FK_Orphan_Sponsor_SponsorId",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropForeignKey(
                name: "FK_OrphanPaymentItem_OrphanPayment_OrphanPaymentId",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrphanPaymentItem_Orphan_OrphanId",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropForeignKey(
                name: "FK_Outgoing_Department_DepartmentId",
                schema: "IIROSA",
                table: "Outgoing");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReport_Charity_CharityId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodicOrphanReport_Orphan_OrphanId",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropForeignKey(
                name: "FK_Provider_Family_FamilyId",
                table: "Provider");

            migrationBuilder.DropForeignKey(
                name: "FK_Region_Country_CountryId",
                table: "Region");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalAidBeneficiary_Family_FamilyId",
                table: "SeasonalAidBeneficiary");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalAidCampaign_Charity_CharityId",
                table: "SeasonalAidCampaign");

            migrationBuilder.DropForeignKey(
                name: "FK_SeasonalAidCampaign_Country_CountryId",
                table: "SeasonalAidCampaign");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsor_Charity_CharityId",
                schema: "IIROSA",
                table: "Sponsor");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsor_City_CityId",
                schema: "IIROSA",
                table: "Sponsor");

            migrationBuilder.DropForeignKey(
                name: "FK_Sponsor_Country_CountryId",
                schema: "IIROSA",
                table: "Sponsor");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_SupportTicketCategory_CategoryId",
                schema: "IIROSA",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_SupportTicketPriority_PriorityId",
                schema: "IIROSA",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTicket_SupportTicketStatus_StatusId",
                schema: "IIROSA",
                table: "SupportTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_TicketResponse_SupportTicket_TicketId",
                schema: "IIROSA",
                table: "TicketResponse");

            migrationBuilder.DropTable(
                name: "Relative");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TicketResponse",
                schema: "IIROSA",
                table: "TicketResponse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicketStatus",
                schema: "Lookup",
                table: "SupportTicketStatus");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicketPriority",
                schema: "Lookup",
                table: "SupportTicketPriority");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicketCategory",
                schema: "Lookup",
                table: "SupportTicketCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SupportTicket",
                schema: "IIROSA",
                table: "SupportTicket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sponsor",
                schema: "IIROSA",
                table: "Sponsor");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PeriodicOrphanReport",
                schema: "IIROSA",
                table: "PeriodicOrphanReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrphanPaymentItem",
                schema: "IIROSA",
                table: "OrphanPaymentItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrphanPayment",
                schema: "IIROSA",
                table: "OrphanPayment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Orphan",
                schema: "IIROSA",
                table: "Orphan");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeProjectType",
                schema: "Lookup",
                table: "OfficeProjectType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeProject",
                schema: "IIROSA",
                table: "OfficeProject");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MissionTimeType",
                schema: "Lookup",
                table: "MissionTimeType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Mission",
                schema: "IIROSA",
                table: "Mission");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Family",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employee",
                schema: "IIROSA",
                table: "Employee");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Department",
                schema: "Lookup",
                table: "Department");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Country",
                schema: "Lookup",
                table: "Country");

            migrationBuilder.DropPrimaryKey(
                name: "PK_City",
                schema: "Lookup",
                table: "City");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Charity",
                schema: "IIROSA",
                table: "Charity");

            migrationBuilder.DropColumn(
                name: "RelativesCount",
                schema: "IIROSA",
                table: "Family");

            migrationBuilder.RenameTable(
                name: "Outgoing",
                schema: "IIROSA",
                newName: "Outgoing");

            migrationBuilder.RenameTable(
                name: "Incoming",
                schema: "IIROSA",
                newName: "Incoming");

            migrationBuilder.RenameTable(
                name: "TicketResponse",
                schema: "IIROSA",
                newName: "TicketResponses");

            migrationBuilder.RenameTable(
                name: "SupportTicketStatus",
                schema: "Lookup",
                newName: "SupportTicketStatuses");

            migrationBuilder.RenameTable(
                name: "SupportTicketPriority",
                schema: "Lookup",
                newName: "SupportTicketPriorities");

            migrationBuilder.RenameTable(
                name: "SupportTicketCategory",
                schema: "Lookup",
                newName: "SupportTicketCategories");

            migrationBuilder.RenameTable(
                name: "SupportTicket",
                schema: "IIROSA",
                newName: "SupportTickets");

            migrationBuilder.RenameTable(
                name: "Sponsor",
                schema: "IIROSA",
                newName: "Sponsors");

            migrationBuilder.RenameTable(
                name: "PeriodicOrphanReport",
                schema: "IIROSA",
                newName: "PeriodicOrphanReports");

            migrationBuilder.RenameTable(
                name: "OrphanPaymentItem",
                schema: "IIROSA",
                newName: "OrphanPaymentItems");

            migrationBuilder.RenameTable(
                name: "OrphanPayment",
                schema: "IIROSA",
                newName: "OrphanPayments");

            migrationBuilder.RenameTable(
                name: "Orphan",
                schema: "IIROSA",
                newName: "Orphans");

            migrationBuilder.RenameTable(
                name: "OfficeProjectType",
                schema: "Lookup",
                newName: "OfficeProjectTypes");

            migrationBuilder.RenameTable(
                name: "OfficeProject",
                schema: "IIROSA",
                newName: "OfficeProjects");

            migrationBuilder.RenameTable(
                name: "MissionTimeType",
                schema: "Lookup",
                newName: "MissionTimeTypes");

            migrationBuilder.RenameTable(
                name: "Mission",
                schema: "IIROSA",
                newName: "Missions");

            migrationBuilder.RenameTable(
                name: "Family",
                schema: "IIROSA",
                newName: "Families");

            migrationBuilder.RenameTable(
                name: "Employee",
                schema: "IIROSA",
                newName: "Employees");

            migrationBuilder.RenameTable(
                name: "Department",
                schema: "Lookup",
                newName: "Departments");

            migrationBuilder.RenameTable(
                name: "Country",
                schema: "Lookup",
                newName: "Countries");

            migrationBuilder.RenameTable(
                name: "City",
                schema: "Lookup",
                newName: "Cities");

            migrationBuilder.RenameTable(
                name: "Charity",
                schema: "IIROSA",
                newName: "Charities");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponse_TicketId",
                table: "TicketResponses",
                newName: "IX_TicketResponses_TicketId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponse_RespondedByUserId",
                table: "TicketResponses",
                newName: "IX_TicketResponses_RespondedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponse_IsInternalNote",
                table: "TicketResponses",
                newName: "IX_TicketResponses_IsInternalNote");

            migrationBuilder.RenameIndex(
                name: "IX_TicketResponse_CreatedOn",
                table: "TicketResponses",
                newName: "IX_TicketResponses_CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketStatus_SortOrder",
                table: "SupportTicketStatuses",
                newName: "IX_SupportTicketStatuses_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketStatus_IsTerminalStatus",
                table: "SupportTicketStatuses",
                newName: "IX_SupportTicketStatuses_IsTerminalStatus");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketStatus_IsActive",
                table: "SupportTicketStatuses",
                newName: "IX_SupportTicketStatuses_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketPriority_SeverityLevel",
                table: "SupportTicketPriorities",
                newName: "IX_SupportTicketPriorities_SeverityLevel");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketPriority_IsActive",
                table: "SupportTicketPriorities",
                newName: "IX_SupportTicketPriorities_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketCategory_SortOrder",
                table: "SupportTicketCategories",
                newName: "IX_SupportTicketCategories_SortOrder");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicketCategory_IsActive",
                table: "SupportTicketCategories",
                newName: "IX_SupportTicketCategories_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_StatusId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_StatusId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_ResolvedOn",
                table: "SupportTickets",
                newName: "IX_SupportTickets_ResolvedOn");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_PriorityId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_PriorityId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_IsSolved",
                table: "SupportTickets",
                newName: "IX_SupportTickets_IsSolved");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_CreatedOn",
                table: "SupportTickets",
                newName: "IX_SupportTickets_CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_CreatedByUserId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_CreatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_CategoryId",
                table: "SupportTickets",
                newName: "IX_SupportTickets_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_SupportTicket_AssignedTo",
                table: "SupportTickets",
                newName: "IX_SupportTickets_AssignedTo");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsor_Email",
                table: "Sponsors",
                newName: "IX_Sponsors_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsor_CountryId",
                table: "Sponsors",
                newName: "IX_Sponsors_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsor_Code",
                table: "Sponsors",
                newName: "IX_Sponsors_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsor_CityId",
                table: "Sponsors",
                newName: "IX_Sponsors_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Sponsor_CharityId",
                table: "Sponsors",
                newName: "IX_Sponsors_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReport_ReportYear",
                table: "PeriodicOrphanReports",
                newName: "IX_PeriodicOrphanReports_ReportYear");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReport_ReportMonth",
                table: "PeriodicOrphanReports",
                newName: "IX_PeriodicOrphanReports_ReportMonth");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReport_OrphanId_ReportMonth_ReportYear",
                table: "PeriodicOrphanReports",
                newName: "IX_PeriodicOrphanReports_OrphanId_ReportMonth_ReportYear");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReport_OrphanId",
                table: "PeriodicOrphanReports",
                newName: "IX_PeriodicOrphanReports_OrphanId");

            migrationBuilder.RenameIndex(
                name: "IX_PeriodicOrphanReport_CharityId",
                table: "PeriodicOrphanReports",
                newName: "IX_PeriodicOrphanReports_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPaymentItem_OrphanPaymentId_OrphanId",
                table: "OrphanPaymentItems",
                newName: "IX_OrphanPaymentItems_OrphanPaymentId_OrphanId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPaymentItem_OrphanPaymentId",
                table: "OrphanPaymentItems",
                newName: "IX_OrphanPaymentItems_OrphanPaymentId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPaymentItem_OrphanId",
                table: "OrphanPaymentItems",
                newName: "IX_OrphanPaymentItems_OrphanId");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_ShowOrder",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_ShowOrder");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_PaymentPeriodTo",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_PaymentPeriodTo");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_PaymentPeriodFrom",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_PaymentPeriodFrom");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_IsBatchUploaded",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_IsBatchUploaded");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_GroupName",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_GroupName");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_GroupDate",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_GroupDate");

            migrationBuilder.RenameIndex(
                name: "IX_OrphanPayment_BatchNo",
                table: "OrphanPayments",
                newName: "IX_OrphanPayments_BatchNo");

            migrationBuilder.RenameIndex(
                name: "IX_Orphan_SponsorId",
                table: "Orphans",
                newName: "IX_Orphans_SponsorId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphan_HealthStatusId",
                table: "Orphans",
                newName: "IX_Orphans_HealthStatusId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphan_FamilyId",
                table: "Orphans",
                newName: "IX_Orphans_FamilyId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphan_EducationLevelId",
                table: "Orphans",
                newName: "IX_Orphans_EducationLevelId");

            migrationBuilder.RenameIndex(
                name: "IX_Orphan_Code",
                table: "Orphans",
                newName: "IX_Orphans_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Orphan_CharityId",
                table: "Orphans",
                newName: "IX_Orphans_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjectType_TypeCode",
                table: "OfficeProjectTypes",
                newName: "IX_OfficeProjectTypes_TypeCode");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProjectType_IsActive",
                table: "OfficeProjectTypes",
                newName: "IX_OfficeProjectTypes_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_ProjectName",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_ProjectName");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_ProjectEndDate",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_ProjectEndDate");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_ProjectDate_IsFinished",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_ProjectDate_IsFinished");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_ProjectDate",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_ProjectDate");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_IsFinished",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_IsFinished");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_RegionId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_ProjectReportFileId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_ProjectReportFileId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_OfficeProjectTypeId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_OfficeProjectTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_CountryId_FK_RegionId_FK_CenterId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_CountryId_FK_RegionId_FK_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_CountryId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_CharityId_IsFinished",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_CharityId_IsFinished");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_CharityId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_CenterId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeProject_FK_AttachedFileId",
                table: "OfficeProjects",
                newName: "IX_OfficeProjects_FK_AttachedFileId");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_MissionDate_IsMissionCompleted",
                table: "Missions",
                newName: "IX_Missions_MissionDate_IsMissionCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_MissionDate",
                table: "Missions",
                newName: "IX_Missions_MissionDate");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_MissionCompletedDate",
                table: "Missions",
                newName: "IX_Missions_MissionCompletedDate");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_IsMissionCompleted",
                table: "Missions",
                newName: "IX_Missions_IsMissionCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_UserId_IsMissionCompleted",
                table: "Missions",
                newName: "IX_Missions_FK_UserId_IsMissionCompleted");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_UserId",
                table: "Missions",
                newName: "IX_Missions_FK_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_RegionId",
                table: "Missions",
                newName: "IX_Missions_FK_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_MissionTypeId",
                table: "Missions",
                newName: "IX_Missions_FK_MissionTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_MissionTimeTypeId",
                table: "Missions",
                newName: "IX_Missions_FK_MissionTimeTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_CountryId",
                table: "Missions",
                newName: "IX_Missions_FK_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Mission_FK_CenterId",
                table: "Missions",
                newName: "IX_Missions_FK_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Family_LivingConditionId",
                table: "Families",
                newName: "IX_Families_LivingConditionId");

            migrationBuilder.RenameIndex(
                name: "IX_Family_HousingTypeId",
                table: "Families",
                newName: "IX_Families_HousingTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Family_CountryId",
                table: "Families",
                newName: "IX_Families_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Family_Code",
                table: "Families",
                newName: "IX_Families_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Family_CityId",
                table: "Families",
                newName: "IX_Families_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Family_CharityId",
                table: "Families",
                newName: "IX_Families_CharityId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_NationalId",
                table: "Employees",
                newName: "IX_Employees_NationalId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_Email",
                table: "Employees",
                newName: "IX_Employees_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_DepartmentId",
                table: "Employees",
                newName: "IX_Employees_DepartmentId");

            migrationBuilder.RenameIndex(
                name: "IX_Employee_Code",
                table: "Employees",
                newName: "IX_Employees_Code");

            migrationBuilder.RenameIndex(
                name: "IX_City_CountryId1",
                table: "Cities",
                newName: "IX_Cities_CountryId1");

            migrationBuilder.RenameIndex(
                name: "IX_City_CountryId",
                table: "Cities",
                newName: "IX_Cities_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_UserId",
                table: "Charities",
                newName: "IX_Charities_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_RegionId",
                table: "Charities",
                newName: "IX_Charities_RegionId");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_Name",
                table: "Charities",
                newName: "IX_Charities_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_IsLocked",
                table: "Charities",
                newName: "IX_Charities_IsLocked");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_IsActive",
                table: "Charities",
                newName: "IX_Charities_IsActive");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_Email",
                table: "Charities",
                newName: "IX_Charities_Email");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_CountryId",
                table: "Charities",
                newName: "IX_Charities_CountryId");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_Code",
                table: "Charities",
                newName: "IX_Charities_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_CityId",
                table: "Charities",
                newName: "IX_Charities_CityId");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_CenterId",
                table: "Charities",
                newName: "IX_Charities_CenterId");

            migrationBuilder.RenameIndex(
                name: "IX_Charity_BankId",
                table: "Charities",
                newName: "IX_Charities_BankId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TicketResponses",
                table: "TicketResponses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicketStatuses",
                table: "SupportTicketStatuses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicketPriorities",
                table: "SupportTicketPriorities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTicketCategories",
                table: "SupportTicketCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SupportTickets",
                table: "SupportTickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sponsors",
                table: "Sponsors",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PeriodicOrphanReports",
                table: "PeriodicOrphanReports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrphanPaymentItems",
                table: "OrphanPaymentItems",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrphanPayments",
                table: "OrphanPayments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Orphans",
                table: "Orphans",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeProjectTypes",
                table: "OfficeProjectTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeProjects",
                table: "OfficeProjects",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MissionTimeTypes",
                table: "MissionTimeTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Missions",
                table: "Missions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Families",
                table: "Families",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Departments",
                table: "Departments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Countries",
                table: "Countries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cities",
                table: "Cities",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Charities",
                table: "Charities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Center_Countries_CountryId",
                table: "Center",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

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
                name: "FK_Charities_Countries_CountryId",
                table: "Charities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Charities_Region_RegionId",
                table: "Charities",
                column: "RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_CountryId",
                table: "Cities",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_CountryId1",
                table: "Cities",
                column: "CountryId1",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Departments_DepartmentId",
                table: "Employees",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Charities_CharityId",
                table: "Families",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Cities_CityId",
                table: "Families",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_Countries_CountryId",
                table: "Families",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Families_HousingType_HousingTypeId",
                table: "Families",
                column: "HousingTypeId",
                principalTable: "HousingType",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Families_LivingCondition_LivingConditionId",
                table: "Families",
                column: "LivingConditionId",
                principalTable: "LivingCondition",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Father_Families_FamilyId",
                table: "Father",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Charities_CharityId",
                table: "HousingProject",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Countries_CountryId",
                table: "HousingProject",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HousingProject_Families_FamilyId",
                table: "HousingProject",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Incoming_Departments_DepartmentId",
                table: "Incoming",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_ApplicationUser_FK_UserId",
                table: "Missions",
                column: "FK_UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Center_FK_CenterId",
                table: "Missions",
                column: "FK_CenterId",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Countries_FK_CountryId",
                table: "Missions",
                column: "FK_CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MissionTimeTypes_FK_MissionTimeTypeId",
                table: "Missions",
                column: "FK_MissionTimeTypeId",
                principalTable: "MissionTimeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_MissionType_FK_MissionTypeId",
                table: "Missions",
                column: "FK_MissionTypeId",
                principalTable: "MissionType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Missions_Region_FK_RegionId",
                table: "Missions",
                column: "FK_RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mother_Families_FamilyId",
                table: "Mother",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProjects_Center_FK_CenterId",
                table: "OfficeProjects",
                column: "FK_CenterId",
                principalTable: "Center",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProjects_Charities_FK_CharityId",
                table: "OfficeProjects",
                column: "FK_CharityId",
                principalTable: "Charities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProjects_Countries_FK_CountryId",
                table: "OfficeProjects",
                column: "FK_CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProjects_OfficeProjectTypes_FK_OfficeProjectTypeId",
                table: "OfficeProjects",
                column: "FK_OfficeProjectTypeId",
                principalTable: "OfficeProjectTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeProjects_Region_FK_RegionId",
                table: "OfficeProjects",
                column: "FK_RegionId",
                principalTable: "Region",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrphanPaymentItems_OrphanPayments_OrphanPaymentId",
                table: "OrphanPaymentItems",
                column: "OrphanPaymentId",
                principalTable: "OrphanPayments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrphanPaymentItems_Orphans_OrphanId",
                table: "OrphanPaymentItems",
                column: "OrphanId",
                principalTable: "Orphans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_Charities_CharityId",
                table: "Orphans",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_EducationLevel_EducationLevelId",
                table: "Orphans",
                column: "EducationLevelId",
                principalTable: "EducationLevel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_Families_FamilyId",
                table: "Orphans",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_HealthStatus_HealthStatusId",
                table: "Orphans",
                column: "HealthStatusId",
                principalTable: "HealthStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orphans_Sponsors_SponsorId",
                table: "Orphans",
                column: "SponsorId",
                principalTable: "Sponsors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Outgoing_Departments_DepartmentId",
                table: "Outgoing",
                column: "DepartmentId",
                principalTable: "Departments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReports_Charities_CharityId",
                table: "PeriodicOrphanReports",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodicOrphanReports_Orphans_OrphanId",
                table: "PeriodicOrphanReports",
                column: "OrphanId",
                principalTable: "Orphans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Provider_Families_FamilyId",
                table: "Provider",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Region_Countries_CountryId",
                table: "Region",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalAidBeneficiary_Families_FamilyId",
                table: "SeasonalAidBeneficiary",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalAidCampaign_Charities_CharityId",
                table: "SeasonalAidCampaign",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SeasonalAidCampaign_Countries_CountryId",
                table: "SeasonalAidCampaign",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsors_Charities_CharityId",
                table: "Sponsors",
                column: "CharityId",
                principalTable: "Charities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsors_Cities_CityId",
                table: "Sponsors",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Sponsors_Countries_CountryId",
                table: "Sponsors",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_SupportTicketCategories_CategoryId",
                table: "SupportTickets",
                column: "CategoryId",
                principalTable: "SupportTicketCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_SupportTicketPriorities_PriorityId",
                table: "SupportTickets",
                column: "PriorityId",
                principalTable: "SupportTicketPriorities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_SupportTicketStatuses_StatusId",
                table: "SupportTickets",
                column: "StatusId",
                principalTable: "SupportTicketStatuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketResponses_SupportTickets_TicketId",
                table: "TicketResponses",
                column: "TicketId",
                principalTable: "SupportTickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
