using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Framework.Core.Migrations
{
    public partial class removeLogsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Log",
                schema: "common");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Log",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CallSite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Exception = table.Column<string>(type: "varchar(6000)", unicode: false, maxLength: 6000, nullable: true),
                    Host = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    LogLevel = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    Logger = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "varchar(4000)", unicode: false, maxLength: 4000, nullable: true),
                    Thread = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    Url = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true),
                    UserName = table.Column<string>(type: "varchar(256)", unicode: false, maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });
        }
    }
}
