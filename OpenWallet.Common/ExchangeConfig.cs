using Newtonsoft.Json;

namespace OpenWallet.Common
{
    public class ExchangeConfig
    {

        [JsonProperty("exchange")]
        public string Exchange { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        [JsonProperty("esecretKey")]
        public string SecretKey { get; set; }

        [JsonProperty("additionnalKey")]
        public string AdditionnalKey { get; set; }
    }

}
