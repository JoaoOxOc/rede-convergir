using System.ComponentModel.DataAnnotations;

namespace UserService.Entities
{
    public class ClientCredential
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string Description { get; set; }
        public string ClientSecret { get; set; }

        #region MetaData

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public string? CreatedBy { get; set; }

        #endregion
    }
}
