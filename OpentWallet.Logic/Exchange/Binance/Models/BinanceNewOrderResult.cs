using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OpentWallet.Logic.Binance
{

    public partial class BinanceNewOrderResult
    {
        public string Symbol { get; set; }

        [JsonProperty("serverTime")]
        public long orderId { get; set; }
        public long orderListId { get; set; }

        public string RateLimitType { get; set; }
        public string clientOrderId { get; set; }
        public long transactTime { get; set; }
        public string price { get; set; }
        public string origQty { get; set; }
        public string executedQty { get; set; }
        public string cummulativeQuoteQty { get; set; }
        public string status { get; set; }
        public string timeInForce { get; set; }
        public string type { get; set; }
        public string side { get; set; }

    }


}
