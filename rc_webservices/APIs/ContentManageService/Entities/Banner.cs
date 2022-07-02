using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContentManageService.Entities
{
    /// <summary>
    /// This entity only stores the non-translatable data of the banner
    /// For translatable data, use BannerTranslation
    /// </summary>
    public class Banner
    {
        [Key]
        public int? Id { get; set; }

        /// <summary>
        /// The identifier of the page where the banner record belongs
        /// </summary>
        [Required]
        [MaxLength(25)]
        public string PageKey { get; set; }

        /// <summary>
        /// The identifier of the component in a page where the banner record belongs
        /// </summary>
        [MaxLength(25)]
        public string? ComponentKey { get; set; }

        /// <summary>
        /// The multi-tier path of this banner, using | as separator
        /// </summary>
        [MaxLength(255)]
        public string? ParentPath { get; set; }

        /// <summary>
        /// Identifies the order position of the banner
        /// Example: in the case of belonging to the same component key, it can be ordered to be an item in a carousel for example
        /// </summary>
        public int? OrderPosition { get; set; }

        public bool? IsDraft { get; set;}

        #region MetaData

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public int? ParentBannerId { get; set; }

        [ForeignKey("ParentBannerId")]
        public Banner? ParentBanner { get; set; }

        public virtual ICollection<BannerTranslation> BannerTranslations { get; set; }

        #endregion

    }
}
