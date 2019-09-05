using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStateTransitions.Models
{
    public class RulesModel
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "rules")]
        public Rule[] Rules { get; set; }
    }

    public class Rule
    {
        [JsonProperty(PropertyName = "ifChildState")]
        public string IfChildState { get; set; }

        [JsonProperty(PropertyName = "notParentStates")]
        public string[] NotParentStates { get; set; }

        [JsonProperty(PropertyName = "setParentStateTo")]
        public string SetParentStateTo { get; set; }

        [JsonProperty(PropertyName = "allChildren")]
        public bool AllChildren { get; set; }
    }
}
