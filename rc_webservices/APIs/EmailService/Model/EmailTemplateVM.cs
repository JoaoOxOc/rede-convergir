using System.Text.Json;

namespace EmailService.Model
{
    public class EmailTemplateVM
    {
        public string Id { get; set; }

        public string Key { get; set; }

        public string? Description { get; set; }

        public string Language { get; set; }

        public string? Subject { get; set; }

        public JsonDocument? TemplateDataJson { get; set; }

        public string? BodyHtml { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
