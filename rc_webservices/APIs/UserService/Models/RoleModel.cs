namespace UserService.Models
{
    public class RoleModel
    {
        public string? RoleId { get; set; }

        public string RoleName { get; set; }

        /// <summary>
        /// In capital letters
        /// </summary>
        public string? NormalizedRoleName { get; set; }

        public List<ScopeModel>? Scopes { get; set; }
    }
}
