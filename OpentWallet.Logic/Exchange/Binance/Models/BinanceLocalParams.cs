using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpentWallet.Logic
{
    public class BinanceLocalParams
    {
        [JsonProperty("pairtoCheck")]
        public List<string> aPairtoCheck { get; set; }
    }
}
