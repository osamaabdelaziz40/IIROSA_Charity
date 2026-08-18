using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Identity.data.migrations
{
    public partial class AddRolesSeed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                schema: "identity",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 303);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .OldAnnotation("Relational:ColumnOrder", 302);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256)
                .OldAnnotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                schema: "identity",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GetDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GetDate()")
                .OldAnnotation("Relational:ColumnOrder", 301);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256)
                .OldAnnotation("Relational:ColumnOrder", 300);

        

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn",  "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("5cc42d23-0b0d-4e38-8547-e4c45c0cf660"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "المدير المباشر لاستضافة", "Hosting Direct Manager", "HostingDirectManager", "HostingDirectManager", null, null });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("e7b00baf-5f44-40eb-939f-d5b090170899"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ادارة الشؤون الإدارية والمرافق", "Administration And Facilities Department", "AdministrationAndFacilitiesDepartment", "AdministrationAndFacilitiesDepartment", null, null });

            migrationBuilder.InsertData(
                schema: "identity",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedBy", "CreatedOn", "DisplayNameAr", "DisplayNameEn", "Name", "NormalizedName", "UpdatedBy", "UpdatedOn" },
                values: new object[] { new Guid("eccd727d-a07d-42f3-b817-535dbd6048fb"), null, "admin", new DateTime(2023, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "موظف", "Employee", "Employee", "Employee", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5cc42d23-0b0d-4e38-8547-e4c45c0cf660"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("e7b00baf-5f44-40eb-939f-d5b090170899"));

            migrationBuilder.DeleteData(
                schema: "identity",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("eccd727d-a07d-42f3-b817-535dbd6048fb"));

            migrationBuilder.DropColumn(
                name: "Department",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Extension",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "JobTitle",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfilePhotoId",
                schema: "identity",
                table: "Users");


            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOn",
                schema: "identity",
                table: "Users",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 303);

            migrationBuilder.AlterColumn<string>(
                name: "UpdatedBy",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 302);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256)
                .Annotation("Relational:ColumnOrder", 2);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOn",
                schema: "identity",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GetDate()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GetDate()")
                .Annotation("Relational:ColumnOrder", 301);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "identity",
                table: "Users",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256)
                .Annotation("Relational:ColumnOrder", 300);
        }
    }
}
