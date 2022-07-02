using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Entities
{
    public class Association
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Alias as an international canonical name
        /// To be used in URIs
        /// </summary>
        public string CanonicalNameAlias { get; set; }

        [Required]
        public string Email { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string Town { get; set; }

        public string PostalCode { get; set; }

        /// <summary>
        /// Using ODS PT data
        /// </summary>
        public string DistrictCode { get; set; }

        /// <summary>
        /// Using ODS PT data
        /// </summary>
        public string MunicipalityCode { get; set; }

        public string Vat { get; set; }

        public string LogoImage { get; set; }

        public string CoverImage { get; set; }

        public string Filename { get; set; }

        public string? Description { get; set; }

        public string? Website { get; set; }

        public string? Tags { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public DateTime? ContractStartDate { get; set; }

        public DateTime? ContractEndDate { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsEmailVerified { get; set; }

        public bool TermsConsent { get; set; }

        public decimal TermsConsentVersion { get; set; }

        [ForeignKey("AssociationType")]
        public int? AssociationTypeId { get; set; }
        public virtual AssociationType? AssociationType { get; set; }

        #region MetaData

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<AssociationProfileTranslation> AssociationProfileTranslations { get; set; }

        #endregion
    }
}
