using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentManageService.Migrations
{
    public partial class seed_registerpage_banner_data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentBannerId",
                table: "Banner",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentPath",
                table: "Banner",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.InsertData(
                table: "Banner",
                columns: new[] { "Id", "ComponentKey", "CreatedAt", "CreatedBy", "IsDraft", "OrderPosition", "PageKey", "ParentBannerId", "ParentPath", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, null, new DateTime(2022, 6, 21, 18, 14, 36, 78, DateTimeKind.Local).AddTicks(1726), null, false, 0, "aboutheadline", null, null, null, null },
                    { 2, null, new DateTime(2022, 6, 21, 18, 14, 36, 78, DateTimeKind.Local).AddTicks(1768), null, false, 0, "registerpage", null, null, null, null }
                });

            migrationBuilder.InsertData(
                table: "Banner",
                columns: new[] { "Id", "ComponentKey", "CreatedAt", "CreatedBy", "IsDraft", "OrderPosition", "PageKey", "ParentBannerId", "ParentPath", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 3, "slider", new DateTime(2022, 6, 21, 18, 14, 36, 78, DateTimeKind.Local).AddTicks(1772), null, false, 0, "registerpage", 2, "|registerpage|", null, null },
                    { 6, "bottomList", new DateTime(2022, 6, 21, 18, 14, 36, 78, DateTimeKind.Local).AddTicks(1780), null, false, 0, "registerpage", 2, "|registerpage|", null, null }
                });

            migrationBuilder.InsertData(
                table: "BannerTranslation",
                columns: new[] { "Id", "BannerDataHtml", "BannerDataJson", "BannerId", "LangKey", "PageKey" },
                values: new object[,]
                {
                    { 1, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 1, "pt-pt", "aboutheadline" },
                    { 2, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 1, "en-us", "aboutheadline" }
                });

            migrationBuilder.InsertData(
                table: "Banner",
                columns: new[] { "Id", "ComponentKey", "CreatedAt", "CreatedBy", "IsDraft", "OrderPosition", "PageKey", "ParentBannerId", "ParentPath", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 4, "slider", new DateTime(2022, 6, 21, 18, 14, 36, 78, DateTimeKind.Local).AddTicks(1775), null, false, 1, "registerpage", 3, "|registerpage|slider|", null, null },
                    { 5, "slider", new DateTime(2022, 6, 21, 18, 14, 36, 78, DateTimeKind.Local).AddTicks(1778), null, false, 2, "registerpage", 3, "|registerpage|slider|", null, null }
                });

            migrationBuilder.InsertData(
                table: "BannerTranslation",
                columns: new[] { "Id", "BannerDataHtml", "BannerDataJson", "BannerId", "LangKey", "PageKey" },
                values: new object[,]
                {
                    { 7, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 6, "pt-pt", "registerpage" },
                    { 8, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 6, "en-us", "registerpage" }
                });

            migrationBuilder.InsertData(
                table: "BannerTranslation",
                columns: new[] { "Id", "BannerDataHtml", "BannerDataJson", "BannerId", "LangKey", "PageKey" },
                values: new object[,]
                {
                    { 3, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 4, "pt-pt", "registerpage" },
                    { 4, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 4, "en-us", "registerpage" },
                    { 5, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 5, "pt-pt", "registerpage" },
                    { 6, null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 5, "en-us", "registerpage" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Banner_ParentBannerId",
                table: "Banner",
                column: "ParentBannerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Banner_Banner_ParentBannerId",
                table: "Banner",
                column: "ParentBannerId",
                principalTable: "Banner",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Banner_Banner_ParentBannerId",
                table: "Banner");

            migrationBuilder.DropIndex(
                name: "IX_Banner_ParentBannerId",
                table: "Banner");

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "ParentBannerId",
                table: "Banner");

            migrationBuilder.DropColumn(
                name: "ParentPath",
                table: "Banner");
        }
    }
}
