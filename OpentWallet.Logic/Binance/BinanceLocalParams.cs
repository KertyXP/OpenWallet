using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpentWallet.Logic
{
    public class BinanceLocalParams
    {
        [JsonProperty("checkcurrenciesToCheck")]
        public bool checkcurrenciesToCheck   { get; set; }
        [JsonProperty("currenciesToCheck")]
        public List<string> aCurrenciesToCheck { get; set; }

        [JsonProperty("pairsToCheck")]
        public List<string> aPairsToCheck { get; set; }
    }
}
