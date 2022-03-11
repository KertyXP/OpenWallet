using System.Collections.Generic;

namespace OpenWallet.Common
{
    public class GlobalConfig
    {
        public List<ExchangeConfig> configs { get; set; }
        public string FavoriteCurrency { get; set; } = "USDT";
        public List<string> FiatMoneys { get; set; } = new List<string>() { "USDT", "EUR", "TUSD", "BUSD", "USD", "DECL" };
    }

}
