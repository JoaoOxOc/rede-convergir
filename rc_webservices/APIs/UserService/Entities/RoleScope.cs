using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Entities
{
    public class RoleScope
    {
        [Key]
        public int Id { get; set; }

        public string RoleId { get; set; }

        [ForeignKey("RoleId")]
        public virtual IdentityRole? IdentityRole { get; set; }

        public int ScopeId { get; set; }

        [ForeignKey("ScopeId")]
        public virtual Scope? Scope { get; set; }
    }
}
