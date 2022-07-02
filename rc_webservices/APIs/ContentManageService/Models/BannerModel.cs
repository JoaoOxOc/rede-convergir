using System.Text.Json;

namespace ContentManageService.Models
{
    public class BannerModel
    {
        public int? Id { get; set; }

        public string PageKey { get; set; }
        public string? ComponentKey { get; set; }
        public int? OrderPosition { get; set; }
        public bool? IsDraft { get; set; }
        public int? ParentBannerId { get; set; }
        public string? ParentBannerPath { get; set; }
        public int? ChildElements { get; set; }

        public List<BannerDataModel>? BannerLanguages { get; set; }

        public class BannerDataModel
        {
            public string LangCode { get; set; }

            public JsonDocument BannerData { get; set; }

            public string? BannerDataHtml { get; set; }
        }
    }
}
