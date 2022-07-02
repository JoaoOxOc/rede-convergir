using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace UserService.Entities
{
    public class TermsConditions
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public decimal Version { get; set; }

        public bool IsActive { get; set; }

        /// <summary>
        /// If deactivated, it means that it had been active in the past - maybe users consented to this version
        /// Shouldn't be deleted/edited if this is true
        /// </summary>
        public bool? BeenActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }

        public string? CreatedBy { get; set; }

        public virtual ICollection<TermsConditionsTranslation> TermsConditionsTranslations { get; set; }

        public TermsConditions ShallowCopy()
        {
            return (TermsConditions)this.MemberwiseClone();
        }
    }
}
