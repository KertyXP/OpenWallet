using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenWallet.Common
{
    public enum Side { Buy, Sell };

    public class GlobalTrade : CurrencySymbolPrice
    {
        public GlobalTrade(string from, string to, double price, string couple, string exchange) : base(from, to, price, couple, exchange)
        {
        }

        public GlobalTrade SetQuantities(double from, double to)
        {
            QuantityFrom = from;
            QuantityTo = to;
            RealQuantityFrom = Couple.StartsWith(From) ? from : to;
            RealQuantityTo = Couple.StartsWith(To) ? from : to;
            return this;
        }

        public string InternalExchangeId { get; set; }
        public double QuantityFrom { get; set; }
        public double QuantityTo { get; set; }

        [JsonProperty]
        public double RealQuantityFrom { get; protected set; }
        [JsonProperty]
        public double RealQuantityTo { get; protected set; }

        public DateTime dtTrade { get; set; }


    }

}
