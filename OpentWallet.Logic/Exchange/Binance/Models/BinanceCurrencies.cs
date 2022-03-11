using Newtonsoft.Json;

namespace OpentWallet.Logic
{
    public class BinanceCurrencies
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }
    }
}
