using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    public partial class new_user_scopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "e932931b-50a4-416e-a513-26617f106699");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "0cdf7867-c55f-4d02-9f46-117e51069de2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "68f26263-cd9c-4218-97b1-3b10d72778fe");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "c8e51a65-7d46-4445-8a23-2dd810f9b007");

            migrationBuilder.InsertData(
                table: "Scope",
                columns: new[] { "Id", "Description", "ScopeName" },
                values: new object[,]
                {
                    { 6, "Read/write of API settings", "settings.admin" },
                    { 7, "Read/write of Email service data like templates", "email.admin" },
                    { 8, "Read only of content management system data", "cms.read" },
                    { 9, "Read/write of content management system data", "cms.write" }
                });

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 3, 5, 15, 46, 19, 696, DateTimeKind.Local).AddTicks(6133));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 3, 5, 15, 46, 19, 696, DateTimeKind.Local).AddTicks(6167));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 3, 5, 15, 46, 19, 696, DateTimeKind.Local).AddTicks(6170));

            migrationBuilder.InsertData(
                table: "RoleScope",
                columns: new[] { "Id", "RoleId", "ScopeId" },
                values: new object[,]
                {
                    { 13, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 6 },
                    { 14, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 7 },
                    { 15, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 8 },
                    { 16, "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf", 9 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "RoleScope",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Scope",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "bc11fb25-ae67-4819-8f9e-feebe55098b7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "71fb43aa-8679-49e8-ba46-4c0f8617bafb");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "d2692ac3-2bdd-4790-909a-719cdd2a697b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "1de98b50-5e0c-4e6e-b223-a3bceb1f707e");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 2, 18, 11, 18, 16, 10, DateTimeKind.Local).AddTicks(1224));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 2, 18, 11, 18, 16, 10, DateTimeKind.Local).AddTicks(1259));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 2, 18, 11, 18, 16, 10, DateTimeKind.Local).AddTicks(1262));
        }
    }
}
