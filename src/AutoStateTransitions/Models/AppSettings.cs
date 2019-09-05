using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStateTransitions.Models
{
    public class AppSettings
    {
        public string PersonalAccessToken { get; set; }
        public string Organization { get; set; }
        public string SourceForRules { get; set; }
    }
}
