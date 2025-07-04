﻿//using System;
//using Newtonsoft.Json;
//using System.Security.Cryptography;
//using System.Text;
//using OpenWallet.Common;
//using System.Net;
//using System.Collections.Generic;
//using System.Linq;
//using OpenWallet.Logic.Abstraction;

//namespace OpentWallet.Logic
//{
//    public class Ascendex : IExchange
//    {
//        private ExchangeConfig oConfig;
//        private const string hostname = "https://ascendex.com";
//        private AscendexAccount oAccount = null;

//        private WebClient client;

//        public string ExchangeCode => "Ascendex";
//        public string ExchangeName => "Ascendex";

//        ExchangeConfig IExchange.oConfig { get; set; }

//        public Ascendex()
//        {
//            client = new WebClient();

//        }

//        public void InitAsync(GlobalConfig oGlobalConfig, ExchangeConfig oConfig)
//        {
//            this.oConfig = oConfig;
//        }

//        private AscendexAccount GetAccount()
//        {
//            if (oAccount != null)
//                return oAccount;


//            var timestampUtcMillisecond = GetNonce();

//            var signature = CalcSignature(timestampUtcMillisecond + "+info", oConfig.SecretKey);

//            string sApi = "/api/pro/v1/info";

//            client.Headers.Add("x-auth-key", oConfig.ApiKey);
//            client.Headers.Add("x-auth-timestamp", timestampUtcMillisecond);
//            client.Headers.Add("x-auth-signature", signature);


//            var request = client.DownloadString($"{hostname}{sApi}");

//            oAccount = JsonConvert.DeserializeObject<AscendexAccount>(request);
//            return oAccount;
//        }

//        public string CalcSignature(string text, string apiSecret)
//        {
//            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret)))
//            {
//                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
//                var b64 = Convert.ToBase64String(hash, 0, hash.Length);
//                return b64;
//            }
//        }

//        private string GetNonce()
//        {
//            var milliseconds = (long)DateTime.Now.ToUniversalTime()
//                .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
//                .TotalMilliseconds;

//            return milliseconds.ToString();
//        }

//        public List<CurrencySymbolPrice> GetCurrencies()
//        {
//            WebClient wc = new WebClient();
//            var sData = wc.DownloadString($"{hostname}/api/pro/v1/ticker");

//            var oCurrencies = JsonConvert.DeserializeObject<AscendexCurrencies>(sData);
//            return oCurrencies.Data.Select(kvp =>
//            {
//                double Price = (kvp.Ask.FirstOrDefault().ToDouble() + kvp.Bid.FirstOrDefault().ToDouble()) / 2;
//                return new List<CurrencySymbolPrice>()
//                {
//                    new CurrencySymbolPrice(kvp.Symbol.Split('/').FirstOrDefault(), kvp.Symbol.Split('/').LatorDefault(), Price, kvp.Symbol, ExchangeName),
//                    new CurrencySymbolPriceReverted(kvp.Symbol.Split('/').FirstOrDefault(), kvp.Symbol.Split('/').LatorDefault(), Price, kvp.Symbol, ExchangeName),
//                };
//            })
//            .SelectMany(o => o)
//            .ToList();
//        }

//        public List<GlobalBalance> GetBalance()
//        {
//            string sApi = $"/{GetAccount().Data.AccountArchive}/api/pro/v1/cash/balance";

//            var timestampUtcMillisecond = GetNonce();

//            var signature = CalcSignature(timestampUtcMillisecond + "+balance", oConfig.SecretKey);

//            client.Headers.Clear();
//            client.Headers.Add("x-auth-key", oConfig.ApiKey);
//            client.Headers.Add("x-auth-timestamp", timestampUtcMillisecond);
//            client.Headers.Add("x-auth-signature", signature);


//            var request2 = client.DownloadString($"{hostname}{sApi}");

//            var balance = JsonConvert.DeserializeObject<AscendexBalence>(request2);


//            return balance.Data.Select(b => new GlobalBalance()
//            {
//                Exchange = ExchangeName,
//                Crypto = b.Asset,
//                Value = b.TotalBalance.ToDouble(),
//            }).Where(b => b.Value > 0).ToList();


//        }
//        public List<GlobalTrade> GetTradeHitory(List<GlobalTrade> aCache, List<GlobalBalance> aAllBalances)
//        {
//            string sApi = $"/{GetAccount().Data.AccountArchive}/api/pro/v2/order/hist";

//            var timestampUtcMillisecond = GetNonce();

//            var signature = CalcSignature(timestampUtcMillisecond + "+order/hist", oConfig.SecretKey);

//            client.Headers.Clear();
//            client.Headers.Add("x-auth-key", oConfig.ApiKey);
//            client.Headers.Add("x-auth-timestamp", timestampUtcMillisecond);
//            client.Headers.Add("x-auth-signature", signature);

//            var request2 = client.DownloadString($"{hostname}{sApi}?account=cash");

//            var balance = JsonConvert.DeserializeObject<AscendexOrderHitory>(request2);

//            List<GlobalTrade> aListTrades = new List<GlobalTrade>(aCache);

//            foreach (var oOrderHitory in balance.Data)
//            {

//                if (aListTrades.Any(lt => lt.InternalExchangeId == oOrderHitory.SeqNum))
//                {
//                    continue;
//                }
//                var cur = new CurrencySymbol(oOrderHitory.Symbol.Split('/').FirstOrDefault(), oOrderHitory.Symbol.Split('/').LatorDefault(), oOrderHitory.Symbol);

//                GlobalTrade globalTrade = null;
//                if (oOrderHitory.Side == "Buy")
//                {

//                    globalTrade = new GlobalTrade(cur.To, cur.From, oOrderHitory.Price.ToDouble(), cur.Couple, ExchangeName);
//                    globalTrade.SetQuantities(oOrderHitory.OrderQty.ToDouble() / globalTrade.Price, oOrderHitory.OrderQty.ToDouble());
//                }
//                else
//                {
//                    globalTrade = new GlobalTrade(cur.From, cur.To, oOrderHitory.Price.ToDouble(), cur.Couple, ExchangeName);
//                    globalTrade.SetQuantities(oOrderHitory.OrderQty.ToDouble(), oOrderHitory.OrderQty.ToDouble() * globalTrade.Price);
//                }
//                globalTrade.InternalExchangeId = oOrderHitory.SeqNum;
//                globalTrade.dtTrade = UnixTimeStampToDateTime(oOrderHitory.LastExecTime / 1000);
//                aListTrades.Add(globalTrade);

//            }

//            return aListTrades;

//        }

//        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
//        {
//            // Unix timestamp is seconds past epoch
//            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
//            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
//            return dtDateTime;
//        }

//        public GlobalTrade PlaceMarketOrder(CurrencySymbol symbol, double quantity, SellBuy SellOrBuy, bool bTest)
//        {
//            throw new NotImplementedException();
//        }
//    }


//    public partial class AscendexOrderHitory
//    {
//        [JsonProperty("code")]
//        public long Code { get; set; }

//        [JsonProperty("data")]
//        public List<Datum> Data { get; set; }
//    }

//    public partial class Datum
//    {
//        [JsonProperty("accountId")]
//        public string AccountId { get; set; }

//        [JsonProperty("seqNum")]
//        public string SeqNum { get; set; }

//        [JsonProperty("orderId")]
//        public string OrderId { get; set; }

//        [JsonProperty("createTime")]
//        public long CreateTime { get; set; }

//        [JsonProperty("lastExecTime")]
//        public long LastExecTime { get; set; }

//        [JsonProperty("side")]
//        public string Side { get; set; }

//        [JsonProperty("symbol")]
//        public string Symbol { get; set; }

//        [JsonProperty("orderQty")]
//        public string OrderQty { get; set; }

//        [JsonProperty("orderType")]
//        public string OrderType { get; set; }

//        [JsonProperty("price")]
//        public string Price { get; set; }

//        [JsonProperty("status")]
//        public string Status { get; set; }

//        [JsonProperty("topPrice")]
//        public string topPrice { get; set; }

//        [JsonProperty("fillQty")]
//        public string FillQty { get; set; }

//        [JsonProperty("avgFillPrice")]
//        public string AvgFillPrice { get; set; }

//        [JsonProperty("fee")]
//        public string Fee { get; set; }

//        [JsonProperty("feeAsset")]
//        public string FeeAsset { get; set; }
//    }

//    public partial class AscendexCurrencies
//    {
//        [JsonProperty("code")]
//        public long Code { get; set; }

//        [JsonProperty("data")]
//        public List<AscendexCurrenciesData> Data { get; set; }
//    }

//    public partial class AscendexCurrenciesData
//    {
//        [JsonProperty("symbol")]
//        public string Symbol { get; set; }

//        [JsonProperty("open")]
//        public string Open { get; set; }

//        [JsonProperty("close")]
//        public string Close { get; set; }

//        [JsonProperty("high")]
//        public string High { get; set; }

//        [JsonProperty("low")]
//        public string Low { get; set; }

//        [JsonProperty("volume")]
//        public string Volume { get; set; }

//        [JsonProperty("ask")]
//        public string[] Ask { get; set; }

//        [JsonProperty("bid")]
//        public string[] Bid { get; set; }
//    }

//    public partial class AscendexBalence
//    {
//        [JsonProperty("code")]
//        public long Code { get; set; }

//        [JsonProperty("data")]
//        public AscendexData[] Data { get; set; }
//    }

//    public partial class AscendexData
//    {
//        [JsonProperty("asset")]
//        public string Asset { get; set; }

//        [JsonProperty("totalBalance")]
//        public string TotalBalance { get; set; }

//        [JsonProperty("availableBalance")]
//        public string AvailableBalance { get; set; }
//    }

//    public partial class AscendexAccount
//    {
//        [JsonProperty("code")]
//        public long Code { get; set; }

//        [JsonProperty("data")]
//        public AscendexData Data { get; set; }
//    }

//    public partial class AscendexData
//    {
//        [JsonProperty("email")]
//        public string Email { get; set; }

//        [JsonProperty("accountArchive")]
//        public long AccountArchive { get; set; }

//        [JsonProperty("viewPermission")]
//        public bool ViewPermission { get; set; }

//        [JsonProperty("tradePermission")]
//        public bool TradePermission { get; set; }

//        [JsonProperty("transferPermission")]
//        public bool TransferPermission { get; set; }

//        [JsonProperty("cashAccount")]
//        public string[] CashAccount { get; set; }

//        [JsonProperty("marginAccount")]
//        public string[] MarginAccount { get; set; }

//        [JsonProperty("futuresAccount")]
//        public string[] FuturesAccount { get; set; }

//        [JsonProperty("userUID")]
//        public string UserUid { get; set; }

//        [JsonProperty("expireTime")]
//        public long ExpireTime { get; set; }

//        [JsonProperty("allowedIps")]
//        public object[] AllowedIps { get; set; }

//        [JsonProperty("limitQuota")]
//        public long LimitQuota { get; set; }
//    }
//}
