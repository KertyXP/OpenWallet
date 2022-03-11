using Newtonsoft.Json;

namespace OpentWallet.Logic
{
    public partial class BinancePair
    {
        [JsonProperty("id")]
        public double Id { get; set; }

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("quote")]
        public string Quote { get; set; }

        [JsonProperty("isMarginTrade")]
        public bool IsMarginTrade { get; set; }

        [JsonProperty("isBuyAllowed")]
        public bool IsBuyAllowed { get; set; }

        [JsonProperty("isSellAllowed")]
        public bool IsSellAllowed { get; set; }
    }
}
