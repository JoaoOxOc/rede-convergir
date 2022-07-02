using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    public partial class add_terms_translation_entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataBlocksJson",
                table: "TermsConditions");

            migrationBuilder.CreateTable(
                name: "TermsConditionsTranslation",
                columns: table => new
                {
                    LangKey = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    DataBlocksJson = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    TermsConditionsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermsConditionsTranslation", x => x.LangKey);
                    table.ForeignKey(
                        name: "FK_TermsConditionsTranslation_TermsConditions_TermsConditionsId",
                        column: x => x.TermsConditionsId,
                        principalTable: "TermsConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "e51eb844-2d36-4614-8511-a4ded06df18b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "a2360da3-b1d1-401a-8dee-8d1c22527882");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "1b0ab8a6-6b1d-4ef7-b46b-95496de78fc8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "b887016d-ff26-453c-8a91-76af2bdce4a6");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 19, 17, 24, 39, 406, DateTimeKind.Local).AddTicks(422));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 19, 17, 24, 39, 406, DateTimeKind.Local).AddTicks(455));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 19, 17, 24, 39, 406, DateTimeKind.Local).AddTicks(457));

            migrationBuilder.CreateIndex(
                name: "IX_TermsConditionsTranslation_TermsConditionsId",
                table: "TermsConditionsTranslation",
                column: "TermsConditionsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermsConditionsTranslation");

            migrationBuilder.AddColumn<JsonDocument>(
                name: "DataBlocksJson",
                table: "TermsConditions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "0bfbb883-cd48-46ca-b8d0-432425c2f71e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "958b326d-27a3-4fe3-b869-4845cdb1f588");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "81ff0562-a319-480d-a94b-53a8e4bdf185");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "2d221f69-c0cf-442b-9bc6-4d08020784af");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 14, 13, 54, 20, 620, DateTimeKind.Local).AddTicks(2692));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 14, 13, 54, 20, 620, DateTimeKind.Local).AddTicks(2742));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 14, 13, 54, 20, 620, DateTimeKind.Local).AddTicks(2745));
        }
    }
}
