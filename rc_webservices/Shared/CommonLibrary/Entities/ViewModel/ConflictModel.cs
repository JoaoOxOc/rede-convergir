using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Entities.ViewModel
{
    public enum ConflictType
    {
        InvalidValue = 1,
        DuplicateValue = 2
    }

    public class ConflictModel
    {
        public string Entity { get; set; }

        public string Field { get; set; }

        public ConflictType ConflictType { get; set; }
    }
}
