using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpentWallet.Logic
{
    public class BinanceLocalParams
    {
        [JsonProperty("pairsToCheck")]
        public List<string> aPairsToCheck { get; set; }
    }
}
