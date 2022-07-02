using System;
using System.Collections.Generic;
using System.Text;

namespace UserService.Models.Reports
{
    public class HTMLtoPDFVM
    {
        public string HTMLBody { get; set; }
        public List<int> DocsToAttach { get; set; }
    }
}
