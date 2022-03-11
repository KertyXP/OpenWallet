using Newtonsoft.Json;

namespace OpentWallet.Logic
{
    public partial class Balance
    {
        [JsonProperty("asset")]
        public string Asset { get; set; }

        [JsonProperty("free")]
        public string Free { get; set; }

        [JsonProperty("locked")]
        public string Locked { get; set; }
    }
}
