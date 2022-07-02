using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "First Name is required")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        public string? ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Terms confirm is required")]
        public bool? TermsConfirmed { get; set; }

        [Required(ErrorMessage = "Association Name is required")]
        public string? AssociationName { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Association Email is required")]
        public string? AssociationEmail { get; set; }

        [Required(ErrorMessage = "Association Type is required")]
        public string? AssociationTypeCode { get; set; }
        

        public string? AssociationVat { get; set; }
        public string? AssociationAddress { get; set; }
        public string? AssociationPostalCode { get; set; }
        public string? AssociationTown { get; set; }
        public string? AssociationDistrictCode { get; set; }
        public string? AssociationMunicipalityCode { get; set; }
        public decimal? AssociationLatitude { get; set; }
        public decimal? AssociationLongitude { get; set; }
        public string? AssociationWebsite { get; set; }
        public string? LangCode { get; set; }
        public string? Timezone { get; set; }

        public string? UserRole { get; set; }
    }
}
