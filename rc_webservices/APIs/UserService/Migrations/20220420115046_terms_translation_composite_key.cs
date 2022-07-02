using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    public partial class terms_translation_composite_key : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TermsConditionsTranslation",
                table: "TermsConditionsTranslation");

            migrationBuilder.AddColumn<decimal>(
                name: "TermsConditionsVersion",
                table: "TermsConditionsTranslation",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TermsConditionsTranslation",
                table: "TermsConditionsTranslation",
                columns: new[] { "LangKey", "TermsConditionsVersion" });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "fe072458-e8ac-40c0-bbd6-f1f6b0469673");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "ac064478-9bf8-4df0-ba52-412fe94b3403");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "45075ca2-1529-4e17-8107-74b750391372");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "9303ec0e-b4c0-4263-91be-edd6223aab19");

            migrationBuilder.InsertData(
                table: "Scope",
                columns: new[] { "Id", "Description", "ScopeName" },
                values: new object[,]
                {
                    { 14, "Discourse admin identity", "discourse.admin" },
                    { 15, "Discourse user identity", "discourse.user" }
                });

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 20, 12, 50, 46, 268, DateTimeKind.Local).AddTicks(5967));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 20, 12, 50, 46, 268, DateTimeKind.Local).AddTicks(6018));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 20, 12, 50, 46, 268, DateTimeKind.Local).AddTicks(6022));

            migrationBuilder.InsertData(
                table: "RoleScope",
                columns: new[] { "Id", "RoleId", "ScopeId" },
                values: new object[] { 21, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 14 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TermsConditionsTranslation",
                table: "TermsConditionsTranslation");

            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DropColumn(
                name: "TermsConditionsVersion",
                table: "TermsConditionsTranslation");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TermsConditionsTranslation",
                table: "TermsConditionsTranslation",
                column: "LangKey");

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
        }
    }
}
