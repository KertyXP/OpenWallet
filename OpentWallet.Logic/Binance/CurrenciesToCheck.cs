using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpentWallet.Logic
{
    public class CurrenciesToCheck
    {
        [JsonProperty("currenciesToCheck")]
        public List<string> aCurrenciesToCheck { get; set; }
    }
}
