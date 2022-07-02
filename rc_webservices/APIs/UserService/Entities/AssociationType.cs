using System.ComponentModel.DataAnnotations;

namespace UserService.Entities
{
    public class AssociationType
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string? Code { get; set; }
        public string? Label { get; set; }
        public string? Description { get; set; }
        public bool? VatRequired { get; set; }
        public virtual ICollection<Association>? Associations { get; set; }

        #region Metadata

        public string? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        #endregion
    }
}
