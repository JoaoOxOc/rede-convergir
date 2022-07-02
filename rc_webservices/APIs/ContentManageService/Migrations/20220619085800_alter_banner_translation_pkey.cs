using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ContentManageService.Migrations
{
    public partial class alter_banner_translation_pkey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BannerTranslation",
                table: "BannerTranslation");

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "LangKey",
                keyValue: "en-us");

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "LangKey",
                keyValue: "pt-pt");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "BannerTranslation",
                type: "integer",
                nullable: false,
                defaultValue: 0)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "PageKey",
                table: "BannerTranslation",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ComponentKey",
                table: "Banner",
                type: "character varying(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderPosition",
                table: "Banner",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_BannerTranslation",
                table: "BannerTranslation",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 6, 19, 9, 58, 0, 96, DateTimeKind.Local).AddTicks(7936));

            migrationBuilder.InsertData(
                table: "BannerTranslation",
                columns: new[] { "Id", "BannerDataHtml", "BannerDataJson", "BannerId", "LangKey", "PageKey" },
                values: new object[,]
                {
                    { 1, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 1, "pt-pt", "aboutheadline" },
                    { 2, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 1, "en-us", "aboutheadline" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_BannerTranslation",
                table: "BannerTranslation");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "BannerTranslation");

            migrationBuilder.DropColumn(
                name: "PageKey",
                table: "BannerTranslation");

            migrationBuilder.DropColumn(
                name: "ComponentKey",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "OrderPosition",
                table: "Banner");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BannerTranslation",
                table: "BannerTranslation",
                column: "LangKey");

            migrationBuilder.UpdateData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2022, 4, 4, 14, 35, 27, 649, DateTimeKind.Local).AddTicks(3489));

            migrationBuilder.UpdateData(
                table: "BannerTranslation",
                keyColumn: "LangKey",
                keyValue: "en-us",
                column: "BannerDataJson",
                value: System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()));

            migrationBuilder.UpdateData(
                table: "BannerTranslation",
                keyColumn: "LangKey",
                keyValue: "pt-pt",
                column: "BannerDataJson",
                value: System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()));
        }
    }
}
