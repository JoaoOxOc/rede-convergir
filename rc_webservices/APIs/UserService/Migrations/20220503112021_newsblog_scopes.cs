using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    public partial class newsblog_scopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "4fbf6642-caa2-4840-b384-9600c412963e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "2501c4ba-4047-48ab-8d45-e6f22d8d4311");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "a0468291-5e8e-4f68-af62-0261ab83fbcd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "1aba0372-bfec-4ade-86b0-a4c13299b76d");

            migrationBuilder.InsertData(
                table: "Scope",
                columns: new[] { "Id", "Description", "ScopeName" },
                values: new object[,]
                {
                    { 17, "Administration of news blog service API", "newsblog.admin" },
                    { 18, "User can post on news blog service API", "newsblog.post" }
                });

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 3, 12, 20, 21, 424, DateTimeKind.Local).AddTicks(5547));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 3, 12, 20, 21, 424, DateTimeKind.Local).AddTicks(5583));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 3, 12, 20, 21, 424, DateTimeKind.Local).AddTicks(5586));

            migrationBuilder.InsertData(
                table: "Setting",
                columns: new[] { "Id", "CreatedAt", "DefaultValue", "Description", "Key", "SettingType", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[] { 4, new DateTime(2022, 5, 3, 12, 20, 21, 424, DateTimeKind.Local).AddTicks(5589), "123456", "API key usada para comunicar com o Mailchimp", "MailchimpApiKey", 3, null, null, "123456" });

            migrationBuilder.InsertData(
                table: "RoleScope",
                columns: new[] { "Id", "RoleId", "ScopeId" },
                values: new object[,]
                {
                    { 24, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 17 },
                    { 25, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 18 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "b42a357f-62a3-4f37-8ec2-369b73083fe0");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "4f61a46f-d1e2-430e-90f4-2447b4d29a72");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "ef5463af-d2d2-4bab-9d7d-67f0ccf72a75");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "ad43a258-257f-4b40-969d-8c2f2aa50a6b");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 29, 15, 59, 59, 277, DateTimeKind.Local).AddTicks(5932));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 29, 15, 59, 59, 277, DateTimeKind.Local).AddTicks(5971));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 29, 15, 59, 59, 277, DateTimeKind.Local).AddTicks(5975));
        }
    }
}
