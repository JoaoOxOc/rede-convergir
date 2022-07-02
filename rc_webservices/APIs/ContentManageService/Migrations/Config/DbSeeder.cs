using ContentManageService.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ContentManageService.Migrations.Config
{
    public class DbSeeder
    {
        private readonly ModelBuilder _modelBuilder;

        public DbSeeder(ModelBuilder modelBuilder)
        {
            this._modelBuilder = modelBuilder;
        }


        /// <summary>
        /// Seeds database with data. It creates an insert script when the migration is generated
        /// </summary>
        public void Seed()
        {
            var options = new JsonDocumentOptions
            {
                AllowTrailingCommas = true
            };

            #region Banners

            _modelBuilder.Entity<Banner>().HasData(
                new Banner { Id = 1, IsDraft = false, PageKey = "aboutheadline", OrderPosition = 0, CreatedAt = DateTime.Now },
                new Banner { Id = 2, IsDraft = false, PageKey = "registerpage", OrderPosition = 0, CreatedAt = DateTime.Now },
                new Banner { Id = 3, ParentBannerId = 2, ParentPath = "|registerpage|", IsDraft = false, PageKey = "registerpage", ComponentKey = "slider", OrderPosition = 0, CreatedAt = DateTime.Now },
                new Banner { Id = 4, ParentBannerId = 3, ParentPath = "|registerpage|slider|", IsDraft = false, PageKey = "registerpage", ComponentKey = "slider", OrderPosition = 1, CreatedAt = DateTime.Now },
                new Banner { Id = 5, ParentBannerId = 3, ParentPath = "|registerpage|slider|", IsDraft = false, PageKey = "registerpage", ComponentKey = "slider", OrderPosition = 2, CreatedAt = DateTime.Now },
                new Banner { Id = 6, ParentBannerId = 2, ParentPath = "|registerpage|", IsDraft = false, PageKey = "registerpage", ComponentKey = "bottomList", OrderPosition = 0, CreatedAt = DateTime.Now }
            );

            _modelBuilder.Entity<BannerTranslation>().HasData(
                new BannerTranslation { Id=1, LangKey = "pt-pt", PageKey = "aboutheadline", BannerId = 1, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions{ AllowTrailingCommas = true }) },
                new BannerTranslation { Id=2, LangKey = "en-us", PageKey = "aboutheadline", BannerId = 1, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) },
                new BannerTranslation { Id = 3, LangKey = "pt-pt", PageKey = "registerpage", BannerId = 4, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) },
                new BannerTranslation { Id = 4, LangKey = "en-us", PageKey = "registerpage", BannerId = 4, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) },
                new BannerTranslation { Id = 5, LangKey = "pt-pt", PageKey = "registerpage", BannerId = 5, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) },
                new BannerTranslation { Id = 6, LangKey = "en-us", PageKey = "registerpage", BannerId = 5, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) },
                new BannerTranslation { Id = 7, LangKey = "pt-pt", PageKey = "registerpage", BannerId = 6, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner cabeçalho\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"primeiro elemento\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"texto do primeiro elemento\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) },
                new BannerTranslation { Id = 8, LangKey = "en-us", PageKey = "registerpage", BannerId = 6, BannerDataJson = JsonDocument.Parse("{\n    \"blocks\": [\n        {\n            \"id\" : \"VDwuz33oJn\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"banner header\",\n                \"level\" : 2\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"header\",\n            \"data\" : {\n                \"text\" : \"first item\",\n                \"level\" : 3\n            }\n        },\n        {\n            \"id\" : \"vFad3244\",\n            \"type\" : \"paragraph\",\n            \"data\" : {\n                \"text\" : \"first item text\",\n                \"headerId\" : \"vFad3244\"\n            }\n        }\n    ]\n}", new JsonDocumentOptions { AllowTrailingCommas = true }) }
            );

            #endregion


        }
    }
}
