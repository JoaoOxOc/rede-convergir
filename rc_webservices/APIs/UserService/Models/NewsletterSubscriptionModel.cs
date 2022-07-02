using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class NewsletterSubscriptionModel
    {
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }

        public string? UserId { get; set; }

        public string? UserFullName { get; set; }

        public string? ValidationToken { get; set; }
    }
}
