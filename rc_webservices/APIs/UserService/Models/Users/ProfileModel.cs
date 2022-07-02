namespace UserService.Models
{
    public class ProfileModel
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? UserRole { get; set; }
        public string? LangCode { get; set; }
        public string? Timezone { get; set; }

        public string? AssociationName { get; set; }
        public string? AssociationLogoImage { get; set; }
        public int? AssociationId { get; set; }

        public bool? IsVerified { get; set; }
        public bool? IsEmailVerified { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<string>? UserRoles { get; set; }
        public string? JobTitle { get; set; }
        public int? Posts { get; set; }
        public string? Location { get; set; }
        public string? Avatar { get; set; }
        public string? CoverImg { get; set; }
        public int? Followers { get; set; }
        public string? Description { get; set; }
    }
}
