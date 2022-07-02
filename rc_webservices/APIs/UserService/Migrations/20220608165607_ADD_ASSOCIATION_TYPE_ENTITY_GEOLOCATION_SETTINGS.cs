using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserService.Migrations
{
    public partial class ADD_ASSOCIATION_TYPE_ENTITY_GEOLOCATION_SETTINGS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssociationTypeId",
                table: "Association",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssociationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "text", nullable: false),
                    Label = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    VatRequired = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssociationType", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "ac8052a3-18b3-4de0-bbe7-96f33c78fbcd");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "727fc333-9e4d-4cf0-a122-550953b76604");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "02bc9819-fba5-4f8f-a60c-dfe4e43ff877");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "f401de22-7967-4b4a-942a-d5e8b115a771");

            migrationBuilder.InsertData(
                table: "AssociationType",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "Description", "Label", "UpdatedAt", "UpdatedBy", "VatRequired" },
                values: new object[,]
                {
                    { 1, "transition_initiative", new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1327), null, "Identifica que é iniciativa local da transição", "Iniciativa de Transição Local", null, null, false },
                    { 2, "association_foundation", new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1330), null, "Identifica que é uma associação ou cooperativa ou fundação", "Associação/Cooperativa/Fundação", null, null, false },
                    { 3, "movement_initiative", new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1332), null, "Identifica que é um movimento sem entidade formal", "Movimento", null, null, false },
                    { 4, "store", new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1334), null, "Identifica que é um comércio sustentável", "Comércio Sustentável", null, null, false }
                });

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1209));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1240));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1243));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1290));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1293));

            migrationBuilder.InsertData(
                table: "Setting",
                columns: new[] { "Id", "CreatedAt", "DefaultValue", "Description", "Key", "SettingType", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { 6, new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1296), "HERE", "Escolha o serviço 'HERE', 'PositionStack' ou 'Google' - preencha a respectiva API key", "GeocodeServiceInUse", 3, null, null, "HERE" },
                    { 7, new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1299), "123456", "API key do serviço de geolocalização da 'HERE'", "HEREgeocodeApiKey", 3, null, null, "123456" },
                    { 8, new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1302), "123456", "API key do serviço de geolocalização da 'PositionStack'", "PositionStackGeocodeApiKey", 3, null, null, "123456" },
                    { 9, new DateTime(2022, 6, 8, 17, 54, 8, 565, DateTimeKind.Local).AddTicks(1304), "123456", "API key do serviço de geolocalização da 'Google'", "GoogleGeocodeApiKey", 3, null, null, "123456" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Association_AssociationTypeId",
                table: "Association",
                column: "AssociationTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Association_AssociationType_AssociationTypeId",
                table: "Association",
                column: "AssociationTypeId",
                principalTable: "AssociationType",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Association_AssociationType_AssociationTypeId",
                table: "Association");

            migrationBuilder.DropTable(
                name: "AssociationType");

            migrationBuilder.DropIndex(
                name: "IX_Association_AssociationTypeId",
                table: "Association");

            migrationBuilder.DeleteData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DropColumn(
                name: "AssociationTypeId",
                table: "Association");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "179642d9-0f10-4d7d-a1a0-b485b3f6659f",
                column: "ConcurrencyStamp",
                value: "41898d50-7a71-4df9-b8aa-2bd8f8832f8a");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "244d3f1a-8594-4adb-9c59-5ec36fcdbf03",
                column: "ConcurrencyStamp",
                value: "7cd4bf2f-bd56-4c9a-ba39-aea34b7ea5e7");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "69d5274f-235d-4013-bbac-0c4eddf31ecc",
                column: "ConcurrencyStamp",
                value: "7b5d8fe8-974a-485c-9cec-d556b8e625c3");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e762fd61-0f58-4c5d-ad0e-7bd322ae3ccf",
                column: "ConcurrencyStamp",
                value: "746c7c62-559d-474e-bb0b-888a5d17a14c");

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 25, 13, 23, 34, 450, DateTimeKind.Local).AddTicks(9744));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 25, 13, 23, 34, 450, DateTimeKind.Local).AddTicks(9787));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 25, 13, 23, 34, 450, DateTimeKind.Local).AddTicks(9790));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 4,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 25, 13, 23, 34, 450, DateTimeKind.Local).AddTicks(9793));

            migrationBuilder.UpdateData(
                table: "Setting",
                keyColumn: "Id",
                keyValue: 5,
                column: "CreatedAt",
                value: new DateTime(2022, 5, 25, 13, 23, 34, 450, DateTimeKind.Local).AddTicks(9796));
        }
    }
}
