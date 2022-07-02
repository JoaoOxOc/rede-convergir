using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CommonLibrary.Entities
{
    public class Setting
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string Key { get; set; }

        public string Description { get; set; }

        [Required]
        public string DefaultValue { get; set; }

        public string Value { get; set; }

        [Required]
        public int SettingType { get; set; }

        #region MetaData

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        #endregion
    }
}
