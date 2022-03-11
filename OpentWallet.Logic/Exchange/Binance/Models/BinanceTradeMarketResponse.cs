using Newtonsoft.Json;

namespace OpentWallet.Logic.Binance
{
    public partial class BinanceTradeMarketResponse
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("orderId")]
        public long OrderId { get; set; }

        [JsonProperty("orderListId")]
        public long OrderListId { get; set; }

        [JsonProperty("clientOrderId")]
        public string ClientOrderId { get; set; }

        [JsonProperty("transactTime")]
        public long TransactTime { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("origQty")]
        public string OrigQty { get; set; }

        [JsonProperty("executedQty")]
        public string ExecutedQty { get; set; }

        [JsonProperty("cummulativeQuoteQty")]
        public string CummulativeQuoteQty { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("side")]
        public string Side { get; set; }

        [JsonProperty("fills")]
        public Fill[] Fills { get; set; }
    }

    public partial class Fill
    {
        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("qty")]
        public string Qty { get; set; }

        [JsonProperty("commission")]
        public string Commission { get; set; }

        [JsonProperty("commissionAsset")]
        public string CommissionAsset { get; set; }

        [JsonProperty("tradeId")]
        public long TradeId { get; set; }
    }
}
