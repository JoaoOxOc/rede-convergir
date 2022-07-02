using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    public partial class add_association_ODS_and_translations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImage",
                table: "Association",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DistrictCode",
                table: "Association",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MunicipalityCode",
                table: "Association",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AssociationProfileTranslation",
                columns: table => new
                {
                    LangKey = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    PageContentKey = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    AssociationId = table.Column<int>(type: "integer", nullable: false),
                    DataBlocksJson = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociationProfileTranslation", x => new { x.LangKey, x.PageContentKey });
                    table.ForeignKey(
                        name: "FK_AssociationProfileTranslation_Association_AssociationId",
                        column: x => x.AssociationId,
                        principalTable: "Association",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "243c71fc-bfc2-4ceb-a2c8-95e9c7ccbac8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "a7dd2996-18ba-4b66-a6d1-d860ebf6d79f");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "3cd72b8c-567c-426a-a93e-0538cb9abcba");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "ea79adc7-fafc-410b-bd99-350997c6c247");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 24, 15, 34, 1, 670, DateTimeKind.Local).AddTicks(5184));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 24, 15, 34, 1, 670, DateTimeKind.Local).AddTicks(5226));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 24, 15, 34, 1, 670, DateTimeKind.Local).AddTicks(5230));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 24, 15, 34, 1, 670, DateTimeKind.Local).AddTicks(5234));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 24, 15, 34, 1, 670, DateTimeKind.Local).AddTicks(5237));

            migrationBuilder.CreateIndex(
                name: "IX_AssociationProfileTranslation_AssociationId",
                table: "AssociationProfileTranslation",
                column: "AssociationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssociationProfileTranslation");

            migrationBuilder.DropColumn(
                name: "CoverImage",
                table: "Association");

            migrationBuilder.DropColumn(
                name: "DistrictCode",
                table: "Association");

            migrationBuilder.DropColumn(
                name: "MunicipalityCode",
                table: "Association");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "17c82971-d22a-4852-bc97-4814683d10c1");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "1352d3dc-08bd-4743-9dbd-deee14e362c4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "d68f1381-810f-422e-8722-0f4b310547a3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "92342bda-6eaf-478c-9de0-398ee66961e0");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 16, 14, 57, 55, 238, DateTimeKind.Local).AddTicks(6845));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 16, 14, 57, 55, 238, DateTimeKind.Local).AddTicks(6883));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 16, 14, 57, 55, 238, DateTimeKind.Local).AddTicks(6887));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 16, 14, 57, 55, 238, DateTimeKind.Local).AddTicks(6889));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 16, 14, 57, 55, 238, DateTimeKind.Local).AddTicks(6932));
        }
    }
}
