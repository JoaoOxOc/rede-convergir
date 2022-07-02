using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLibrary.Entities.ViewModel
{
    public class ExceptionModel
    {
        public string? Message { get; set; }
        public string? StackTrace { get; set; }
        public Exception? InnerException { get; set; }
        public DateTime? DateLogging { get; set; }
        public string? InputDataJson { get; set; }
        public string? AdminRole { get; set; }
        public string? UserId { get; set; }
    }
}
