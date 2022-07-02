namespace UserService.Models
{
    public class UserReadModel
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? UserRole { get; set; }
        public string? LangCode { get; set; }
        public string? Timezone { get; set; }

        public string? AssociationName { get; set; }
        public string? AssociationLogoImage { get; set; }
        public int? AssociationId { get; set; }
        public string? PhoneNumber { get; set; }

        public bool? IsVerified { get; set; }
        public bool? IsEmailVerified { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public List<string>? UserRoles { get; set; }
    }
}
