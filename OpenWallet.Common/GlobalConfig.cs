using Newtonsoft.Json;
using System.Collections.Generic;

namespace OpenWallet.Common
{
    public class GlobalConfig
    {
        [JsonProperty("configs")]
        public List<ExchangeConfig> aConfigs { get; set; }
    }

}
