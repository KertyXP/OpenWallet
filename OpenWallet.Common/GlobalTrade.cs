using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace OpenWallet.Common
{
    public enum Side { Buy, Sell };

    public class GlobalTrade : CurrencySymbolPrice
    {
        public GlobalTrade()
        {
        }

        public string InternalExchangeId { get; set; }
        public double QuantityFrom { get; set; }
        public double QuantityTo { get; set; }
        public DateTime dtTrade { get; set; }


    }

}
