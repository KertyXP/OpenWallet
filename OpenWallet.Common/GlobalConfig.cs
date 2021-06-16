using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenWallet.Common
{
    public class GlobalConfig
    {
        [JsonProperty("configs")]
        public List<ExchangeConfig> aConfigs { get; set; }

        [JsonProperty("favoriteCurrency")]
        public string FavoriteCurrency { get; set; } = "USDT";

        [JsonProperty("fiatMoneys")]
        public List<string> FiatMoneys { get; set; } = new List<string>() { "USDT", "EUR", "TUSD", "BUSD", "USD", "DECL" };
    }

}
