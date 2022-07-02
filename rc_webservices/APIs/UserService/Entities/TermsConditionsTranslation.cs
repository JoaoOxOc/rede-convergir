using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace UserService.Entities
{
    public class TermsConditionsTranslation
    {
        /// <summary>
        /// The ID should be the language identifier, like en-us
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [MaxLength(5)]
        public string LangKey { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public decimal TermsConditionsVersion { get; set; }

        /// <summary>
        /// Store banner data as a json structure
        /// </summary>
        [Column(TypeName = "jsonb")]
        public JsonDocument? DataBlocksJson { get; set; }

        [ForeignKey("TermsConditions")]
        public int? TermsConditionsId { get; set; }
        public virtual TermsConditions TermsConditions { get; set; }

        public TermsConditionsTranslation ShallowCopy()
        {
            return (TermsConditionsTranslation)this.MemberwiseClone();
        }
    }
}
