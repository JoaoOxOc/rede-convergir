using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContentManageService.Migrations
{
    public partial class seed_banner_data : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Banner",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "IsDraft", "PageKey", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, new DateTime(2022, 4, 4, 14, 35, 27, 649, DateTimeKind.Local).AddTicks(3489), null, false, "aboutheadline", null, null });

            migrationBuilder.InsertData(
                table: "BannerTranslation",
                columns: new[] { "LangKey", "BannerDataHtml", "BannerDataJson", "BannerId" },
                values: new object[,]
                {
                    { "en-us", null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 1 },
                    { "pt-pt", null, System.Text.Json.JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new System.Text.Json.JsonDocumentOptions()), 1 }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "LangKey",
                keyValue: "en-us");

            migrationBuilder.DeleteData(
                table: "BannerTranslation",
                keyColumn: "LangKey",
                keyValue: "pt-pt");

            migrationBuilder.DeleteData(
                table: "Banner",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
