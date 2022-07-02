using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UserService.Entities
{
    public class Scope
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ScopeName { get; set; }
        public string Description { get; set; }

        [JsonIgnore]
        public List<RoleScope> RoleScopes { get; set; }
    }
}
