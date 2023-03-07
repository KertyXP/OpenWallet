using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenWallet.Common
{
    public class Interval
    {
        public static string Minute15 => "15m"; 
        public static string Hour1 => "1h";
        public static string Hour2 => "2h"; 
        public static string Hour4 => "4h"; 
        public static string Hour8 => "8h"; 
        public static string Hour12 => "12h"; 
        public static string Hour24 => "1d"; 
    }


    public class TradesData
    {
        public string interval { get; set; }
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
