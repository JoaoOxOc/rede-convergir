using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserService.Migrations
{
    public partial class add_newsletter_subscription_entity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NewsletterSubscription",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "text", nullable: false),
                    SubscriptionToken = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscription", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsletterSubscription_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "65b61a37-6ff0-48ba-92ad-6f71606653a4");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "5dc9d007-0768-4192-9cf0-5be8aef9ba03");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "7961ed14-a19a-467c-96ef-d7f9c2829a4d");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "31055834-0f9b-433c-9120-0b050fae1967");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 23, 12, 7, 50, 160, DateTimeKind.Local).AddTicks(3810));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 23, 12, 7, 50, 160, DateTimeKind.Local).AddTicks(3848));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 23, 12, 7, 50, 160, DateTimeKind.Local).AddTicks(3851));

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscription_UserId",
                table: "NewsletterSubscription",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscription");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "a59f4e4e-1c15-4f26-8da1-14c51dfb2af8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "8891434a-f93f-4141-8b99-0515a5313304");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "4355c6e7-c515-4622-82dc-1ab53221a1b8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "6377daf8-2ed8-4d79-9afa-05ec457a2078");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 22, 10, 34, 58, 886, DateTimeKind.Local).AddTicks(1944));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 22, 10, 34, 58, 886, DateTimeKind.Local).AddTicks(1988));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 22, 10, 34, 58, 886, DateTimeKind.Local).AddTicks(1991));
        }
    }
}
