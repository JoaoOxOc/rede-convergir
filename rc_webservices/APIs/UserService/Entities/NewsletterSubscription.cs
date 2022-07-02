using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Entities
{
    public class NewsletterSubscription
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string Email { get; set; }
        public string? SubscriptionToken { get; set; }

        [ForeignKey("User")]
        public string? UserId { get; set; }
        public virtual User? User { get; set; }

        #region MetaData

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public string? CreatedBy { get; set; }

        #endregion
    }
}
