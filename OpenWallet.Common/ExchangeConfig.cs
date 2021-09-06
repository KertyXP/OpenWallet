using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenWallet.Common
{
    public class ExchangeConfig
    {

        [JsonProperty("exchangeCode")] 
        public string ExchangeCode { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }
        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("esecretKey")] 
        public string SecretKey { get; set; }

        [JsonProperty("additionnalKey")] 
        public string AdditionnalKey { get; set; }

        [JsonProperty("localParams")]
        public JToken LocalParams { get; set; }

        [JsonProperty("currenciesToIgnore")]
        public List<string> CurrenciesToIgnore { get; set; }
    }
}
