using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Migrations
{
    public partial class new_fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Setting",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ClientCredential",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ClientCredential",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ClientCredential",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "ClientCredential",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "9a751937-518d-4dce-a712-c6091d69dfe3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "f6837a6b-aa5f-4537-a80f-cebe1ebf105c");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "dacbfd44-634d-4af7-b796-f109173b1f63");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "ec0f5635-fbe8-4621-aa21-f6703cbc0254");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 3, 12, 17, 35, 12, 122, DateTimeKind.Local).AddTicks(525));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 3, 12, 17, 35, 12, 122, DateTimeKind.Local).AddTicks(591));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 3, 12, 17, 35, 12, 122, DateTimeKind.Local).AddTicks(594));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Setting");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ClientCredential");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ClientCredential");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ClientCredential");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "ClientCredential");

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
        }
    }
}
