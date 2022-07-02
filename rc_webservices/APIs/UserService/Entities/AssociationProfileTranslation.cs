using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace UserService.Entities
{
    public class AssociationProfileTranslation
    {
        /// <summary>
        /// The ID should be the language identifier, like en-us
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [MaxLength(5)]
        public string LangKey { get; set; }

        /// <summary>
        /// The key should be a unique identifier of the widget where the record is consumed on a frontend page
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        [MaxLength(25)]
        public string PageContentKey { get; set; }

        [ForeignKey("Association")]
        [Required]
        public int AssociationId { get; set; }

        /// <summary>
        /// Store profile block data as a json structure
        /// </summary>
        [Column(TypeName = "jsonb")]
        public JsonDocument? DataBlocksJson { get; set; }

        public virtual Association Association { get; set; }
    }
}
