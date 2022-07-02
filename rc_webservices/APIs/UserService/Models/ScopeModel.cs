using UserService.Entities;

namespace UserService.Models
{
    public class ScopeModel
    {
        public int? ScopeId { get; set; }

        public string? ScopeIdentifier { get; set; }

        public string? Description { get; set; }

        public List<RoleScope>? ScopeRoles { get; set; }
    }
}
