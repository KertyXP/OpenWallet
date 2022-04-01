using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenWallet.Common
{

    public class TradesData
    {
        public CurrencySymbolExchange SymbolExchange { get; set; }
        public List<TradeData> Trades { get; set; }
    }

    public class TradeData
    {
        public DateTime dtOpen { get; set; }
        public DateTime dtClose { get; set; }
        public double openPrice { get; set; }
        public double closePrice { get; set; }
        public double lowestPrice { get; set; }
        public double highestPrice { get; set; }
        public int numberOfTrades { get; set; }
        public int volume { get; set; }
        public int assetVolume { get; set; }
    }
}
